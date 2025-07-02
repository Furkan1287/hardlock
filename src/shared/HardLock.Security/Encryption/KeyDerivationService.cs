using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace HardLock.Security.Encryption;

public interface IKeyDerivationService
{
    Task<byte[]> DeriveKeyAsync(string password, byte[] salt, int iterations);
    Task<byte[]> DeriveBiometricKeyAsync(string biometricData, byte[] salt);
    Task<string> HashPasswordAsync(string password);
    Task<bool> VerifyPasswordAsync(string password, string hash);
}

public class KeyDerivationService : IKeyDerivationService
{
    private readonly ILogger<KeyDerivationService> _logger;

    public KeyDerivationService(ILogger<KeyDerivationService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> DeriveKeyAsync(string password, byte[] salt, int iterations)
    {
        try
        {
            _logger.LogDebug("Deriving key with {Iterations} iterations", iterations);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256);

            var key = pbkdf2.GetBytes(32); // 256-bit key for AES-256

            _logger.LogDebug("Key derivation completed successfully");
            return await Task.FromResult(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during key derivation");
            throw new CryptographicException("Key derivation failed", ex);
        }
    }

    public async Task<byte[]> DeriveBiometricKeyAsync(string biometricData, byte[] salt)
    {
        try
        {
            _logger.LogDebug("Deriving biometric key");

            // Use fuzzy extractors for biometric key derivation
            // This is a simplified implementation - in production, use specialized libraries
            var biometricBytes = System.Text.Encoding.UTF8.GetBytes(biometricData);
            var combined = new byte[biometricBytes.Length + salt.Length];
            
            Array.Copy(biometricBytes, 0, combined, 0, biometricBytes.Length);
            Array.Copy(salt, 0, combined, biometricBytes.Length, salt.Length);

            using var sha256 = SHA256.Create();
            var key = sha256.ComputeHash(combined);

            _logger.LogDebug("Biometric key derivation completed");
            return await Task.FromResult(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during biometric key derivation");
            throw new CryptographicException("Biometric key derivation failed", ex);
        }
    }

    public async Task<string> HashPasswordAsync(string password)
    {
        try
        {
            _logger.LogDebug("Hashing password");

            var hash = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));

            _logger.LogDebug("Password hashing completed");
            return await Task.FromResult(hash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password hashing");
            throw new CryptographicException("Password hashing failed", ex);
        }
    }

    public async Task<bool> VerifyPasswordAsync(string password, string hash)
    {
        try
        {
            _logger.LogDebug("Verifying password");

            var isValid = BCrypt.Net.BCrypt.Verify(password, hash);

            _logger.LogDebug("Password verification completed: {IsValid}", isValid);
            return await Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password verification");
            return await Task.FromResult(false);
        }
    }
} 