using HardLock.Identity.Models;

namespace HardLock.Identity.Services;

public interface IUserService
{
    Task<UserResponse> RegisterUserAsync(UserRegistrationRequest request);
    Task<UserResponse?> GetUserByEmailAsync(string email);
    Task<UserResponse?> GetUserByIdAsync(Guid id);
    Task<UserResponse> UpdateUserAsync(Guid id, UserUpdateRequest request);
    Task<bool> ValidatePasswordAsync(string email, string password);
    Task LogoutAsync(Guid userId);
    Task<bool> UserExistsAsync(string email);
} 