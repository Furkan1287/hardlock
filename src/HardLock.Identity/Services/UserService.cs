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
        var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Hash password
        var passwordHash = await _keyDerivationService.HashPasswordAsync(request.Password);

        // Create user
        var user = await _userRepository.CreateUserAsync(request, passwordHash);

        _logger.LogInformation("User registered successfully: {UserId}", user.Id);

        return user;
    }

    public async Task<UserResponse?> GetUserByEmailAsync(string email)
    {
        var cacheKey = $"user:email:{email.ToLowerInvariant()}";
        var cachedUser = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedUser))
        {
            return JsonSerializer.Deserialize<UserResponse>(cachedUser);
        }

        var dbUser = await _userRepository.GetUserByEmailAsync(email);

        if (dbUser != null)
        {
            await CacheUserAsync(dbUser);
        }

        return dbUser;
    }

    public async Task<UserResponse?> GetUserByIdAsync(Guid id)
    {
        var cacheKey = $"user:id:{id}";
        var cachedUser = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedUser))
        {
            return JsonSerializer.Deserialize<UserResponse>(cachedUser);
        }

        var dbUser = await _userRepository.GetUserByIdAsync(id);

        if (dbUser != null)
        {
            await CacheUserAsync(dbUser);
        }

        return dbUser;
    }

    public async Task<UserResponse> UpdateUserAsync(Guid id, UserUpdateRequest request)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var success = await _userRepository.UpdateUserAsync(id, request);
        if (!success)
        {
            throw new InvalidOperationException("Failed to update user");
        }

        // Get updated user
        var updatedUser = await _userRepository.GetUserByIdAsync(id);
        if (updatedUser == null)
        {
            throw new InvalidOperationException("Failed to retrieve updated user");
        }

        // Update cache
        await CacheUserAsync(updatedUser);

        _logger.LogInformation("User updated: {UserId}", id);

        return updatedUser;
    }

    public async Task<bool> ValidatePasswordAsync(string email, string password)
    {
        var passwordHash = await _userRepository.GetPasswordHashAsync(email);
        if (passwordHash == null)
            return false;

        return await _keyDerivationService.VerifyPasswordAsync(password, passwordHash);
    }

    public async Task LogoutAsync(Guid userId)
    {
        // Clear user cache
        await _cache.RemoveAsync($"user:id:{userId}");

        _logger.LogInformation("User logged out: {UserId}", userId);
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        return user != null;
    }

    private async Task CacheUserAsync(UserResponse user)
    {
        var userJson = JsonSerializer.Serialize(user);
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        };

        await _cache.SetStringAsync($"user:id:{user.Id}", userJson, cacheOptions);
        await _cache.SetStringAsync($"user:email:{user.Email}", userJson, cacheOptions);
    }
} 