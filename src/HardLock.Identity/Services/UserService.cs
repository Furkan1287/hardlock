using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using HardLock.Identity.Models;
using HardLock.Identity.Repositories;
using HardLock.Security.Encryption;

namespace HardLock.Identity.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IKeyDerivationService _keyDerivationService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IKeyDerivationService keyDerivationService,
        IDistributedCache cache,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _keyDerivationService = keyDerivationService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<UserResponse> RegisterUserAsync(UserRegistrationRequest request)
    {
        _logger.LogInformation("Registering new user: {Email}", request.Email);

        // Check if user already exists
        if (await UserExistsAsync(request.Email))
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Hash password
        var passwordHash = await _keyDerivationService.HashPasswordAsync(request.Password);

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = request.FirstName,
            LastName = request.LastName,
            RequiresMfa = request.RequiresMfa,
            Roles = new List<string> { "User" },
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        // Cache user data
        await CacheUserAsync(user);

        _logger.LogInformation("User registered successfully: {UserId}", user.Id);

        return MapToUserResponse(user);
    }

    public async Task<UserResponse?> GetUserByEmailAsync(string email)
    {
        var cacheKey = $"user:email:{email.ToLowerInvariant()}";
        var cachedUser = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedUser))
        {
            var user = JsonSerializer.Deserialize<User>(cachedUser);
            return user != null ? MapToUserResponse(user) : null;
        }

        var dbUser = await _userRepository.GetByEmailAsync(email);

        if (dbUser != null)
        {
            await CacheUserAsync(dbUser);
            return MapToUserResponse(dbUser);
        }

        return null;
    }

    public async Task<UserResponse?> GetUserByIdAsync(Guid id)
    {
        var cacheKey = $"user:id:{id}";
        var cachedUser = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedUser))
        {
            var user = JsonSerializer.Deserialize<User>(cachedUser);
            return user != null ? MapToUserResponse(user) : null;
        }

        var dbUser = await _userRepository.GetByIdAsync(id);

        if (dbUser != null)
        {
            await CacheUserAsync(dbUser);
            return MapToUserResponse(dbUser);
        }

        return null;
    }

    public async Task<UserResponse> UpdateUserAsync(Guid id, UserUpdateRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Update fields
        if (!string.IsNullOrEmpty(request.FirstName))
            user.FirstName = request.FirstName;
        
        if (!string.IsNullOrEmpty(request.LastName))
            user.LastName = request.LastName;
        
        if (request.RequiresMfa.HasValue)
            user.RequiresMfa = request.RequiresMfa.Value;

        await _userRepository.UpdateAsync(user);

        // Update cache
        await CacheUserAsync(user);

        _logger.LogInformation("User updated: {UserId}", id);

        return MapToUserResponse(user);
    }

    public async Task<bool> ValidatePasswordAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null)
            return false;

        return await _keyDerivationService.VerifyPasswordAsync(password, user.PasswordHash);
    }

    public async Task LogoutAsync(Guid userId)
    {
        // Clear user cache
        await _cache.RemoveAsync($"user:id:{userId}");

        _logger.LogInformation("User logged out: {UserId}", userId);
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        return await _userRepository.ExistsAsync(email);
    }

    private async Task CacheUserAsync(User user)
    {
        var userJson = JsonSerializer.Serialize(user);
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        };

        await _cache.SetStringAsync($"user:id:{user.Id}", userJson, cacheOptions);
        await _cache.SetStringAsync($"user:email:{user.Email}", userJson, cacheOptions);
    }

    private static UserResponse MapToUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            RequiresMfa = user.RequiresMfa,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = user.Roles
        };
    }
} 