using HardLock.Identity.Models;

namespace HardLock.Identity.Repositories;

public interface IUserRepository
{
    Task<UserResponse?> GetUserByEmailAsync(string email);
    Task<UserResponse?> GetUserByIdAsync(Guid id);
    Task<UserResponse> CreateUserAsync(UserRegistrationRequest request, string hashedPassword);
    Task<bool> UpdateUserAsync(Guid id, UserUpdateRequest request);
    Task<bool> UpdateLastLoginAsync(Guid id);
    Task<string?> GetPasswordHashAsync(string email);
} 