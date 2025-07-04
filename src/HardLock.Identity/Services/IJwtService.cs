using System.Security.Claims;
using HardLock.Identity.Models;

namespace HardLock.Identity.Services;

public interface IJwtService
{
    string GenerateAccessToken(UserResponse user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    bool IsTokenExpired(string token);
} 