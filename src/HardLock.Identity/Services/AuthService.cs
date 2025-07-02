using Microsoft.EntityFrameworkCore;
using HardLock.Identity.Data;
using HardLock.Identity.Models;

namespace HardLock.Identity.Services;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly IdentityDbContext _context;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserService userService,
        IJwtService jwtService,
        IdentityDbContext context,
        ILogger<AuthService> logger)
    {
        _userService = userService;
        _jwtService = jwtService;
        _context = context;
        _logger = logger;
    }

    public async Task<AuthResponse> LoginAsync(UserLoginRequest request)
    {
        _logger.LogInformation("Login attempt for user: {Email}", request.Email);

        // Validate user credentials
        var isValid = await _userService.ValidatePasswordAsync(request.Email, request.Password);
        if (!isValid)
        {
            _logger.LogWarning("Invalid login attempt for user: {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        // Get user details
        var user = await _userService.GetUserByEmailAsync(request.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is deactivated");
        }

        // Handle MFA if required
        if (user.RequiresMfa)
        {
            if (string.IsNullOrEmpty(request.MfaCode))
            {
                throw new UnauthorizedAccessException("MFA code required");
            }

            // TODO: Implement MFA validation
            // For now, we'll just check if a code is provided
            if (request.MfaCode != "123456") // Placeholder
            {
                throw new UnauthorizedAccessException("Invalid MFA code");
            }
        }

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Store refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        // Update last login
        var dbUser = await _context.Users.FindAsync(user.Id);
        if (dbUser != null)
        {
            dbUser.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToInt32(Environment.GetEnvironmentVariable("JWT__ExpiryMinutes") ?? "15")),
            User = user
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        _logger.LogInformation("Token refresh attempt");

        // Find refresh token in database
        var tokenEntity = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

        if (tokenEntity == null || tokenEntity.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        // Get user details
        var user = await _userService.GetUserByIdAsync(tokenEntity.UserId);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("User not found or inactive");
        }

        // Revoke old refresh token
        tokenEntity.IsRevoked = true;

        // Generate new tokens
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        // Store new refresh token
        var newRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Token refreshed successfully for user: {UserId}", user.Id);

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToInt32(Environment.GetEnvironmentVariable("JWT__ExpiryMinutes") ?? "15")),
            User = user
        };
    }
} 