using HardLock.Identity.Models;

namespace HardLock.Identity.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(UserLoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
} 