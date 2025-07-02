using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace HardLock.Security.Encryption;

public interface ITimelockService
{
    Task<TimelockKeyPair> GenerateTimelockKeysAsync();
    Task<string> EncryptFileKeyAsync(string fileKey, string publicKey);
    Task<string?> DecryptFileKeyAsync(string encryptedKey, string privateKey);
    Task<bool> IsTimelockExpiredAsync(DateTime? unlockAt, long? blockNumber);
    Task<TimelockValidationResult> ValidateTimelockAsync(DateTime? unlockAt, long? blockNumber, string? currentBlockHash = null);
}

public class TimelockService : ITimelockService
{
    private readonly ILogger<TimelockService> _logger;
    private readonly IEthereumBlockService _ethereumService;

    public TimelockService(ILogger<TimelockService> logger, IEthereumBlockService ethereumService)
    {
        _logger = logger;
        _ethereumService = ethereumService;
    }

    public async Task<TimelockKeyPair> GenerateTimelockKeysAsync()
    {
        try
        {
            using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384);
            
            var publicKeyBytes = ecdsa.ExportSubjectPublicKeyInfo();
            var privateKeyBytes = ecdsa.ExportECPrivateKey();
            
            var keyPair = new TimelockKeyPair
            {
                PublicKey = Convert.ToBase64String(publicKeyBytes),
                PrivateKey = Convert.ToBase64String(privateKeyBytes),
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Generated ECDSA P-384 timelock key pair");
            return keyPair;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating timelock keys");
            throw;
        }
    }

    public async Task<string> EncryptFileKeyAsync(string fileKey, string publicKey)
    {
        try
        {
            var publicKeyBytes = Convert.FromBase64String(publicKey);
            
            using var ecdsa = ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
            
            // Generate a random AES key for encrypting the file key
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey();
            aes.GenerateIV();
            
            // Encrypt the file key with AES
            using var encryptor = aes.CreateEncryptor();
            var fileKeyBytes = Encoding.UTF8.GetBytes(fileKey);
            var encryptedFileKey = encryptor.TransformFinalBlock(fileKeyBytes, 0, fileKeyBytes.Length);
            
            // Encrypt the AES key with ECDSA public key
            var aesKeyData = new
            {
                Key = Convert.ToBase64String(aes.Key),
                IV = Convert.ToBase64String(aes.IV)
            };
            
            var aesKeyJson = JsonSerializer.Serialize(aesKeyData);
            var aesKeyBytes = Encoding.UTF8.GetBytes(aesKeyJson);
            
            // Use ECDH to derive shared secret (simplified for demo)
            // In production, use proper ECDH key derivation
            var encryptedAesKey = await EncryptWithECDHAsync(aesKeyBytes, ecdsa);
            
            var result = new
            {
                EncryptedFileKey = Convert.ToBase64String(encryptedFileKey),
                EncryptedAesKey = Convert.ToBase64String(encryptedAesKey)
            };
            
            return JsonSerializer.Serialize(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting file key with timelock");
            throw;
        }
    }

    public async Task<string?> DecryptFileKeyAsync(string encryptedKey, string privateKey)
    {
        try
        {
            var privateKeyBytes = Convert.FromBase64String(privateKey);
            
            using var ecdsa = ECDsa.Create();
            ecdsa.ImportECPrivateKey(privateKeyBytes, out _);
            
            var encryptedData = JsonSerializer.Deserialize<dynamic>(encryptedKey);
            var encryptedFileKey = Convert.FromBase64String(encryptedData.GetProperty("EncryptedFileKey").GetString());
            var encryptedAesKey = Convert.FromBase64String(encryptedData.GetProperty("EncryptedAesKey").GetString());
            
            // Decrypt AES key with ECDSA private key
            var aesKeyBytes = await DecryptWithECDHAsync(encryptedAesKey, ecdsa);
            var aesKeyJson = Encoding.UTF8.GetString(aesKeyBytes);
            var aesKeyData = JsonSerializer.Deserialize<dynamic>(aesKeyJson);
            
            // Decrypt file key with AES
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(aesKeyData.GetProperty("Key").GetString());
            aes.IV = Convert.FromBase64String(aesKeyData.GetProperty("IV").GetString());
            
            using var decryptor = aes.CreateDecryptor();
            var decryptedFileKey = decryptor.TransformFinalBlock(encryptedFileKey, 0, encryptedFileKey.Length);
            
            return Encoding.UTF8.GetString(decryptedFileKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting file key with timelock");
            return null;
        }
    }

    public async Task<bool> IsTimelockExpiredAsync(DateTime? unlockAt, long? blockNumber)
    {
        if (!unlockAt.HasValue && !blockNumber.HasValue)
            return true; // No timelock, always allow access
        
        var validation = await ValidateTimelockAsync(unlockAt, blockNumber);
        return validation.IsExpired;
    }

    public async Task<TimelockValidationResult> ValidateTimelockAsync(DateTime? unlockAt, long? blockNumber, string? currentBlockHash = null)
    {
        var result = new TimelockValidationResult
        {
            IsExpired = true,
            ValidationTime = DateTime.UtcNow
        };

        try
        {
            // Check timestamp-based timelock
            if (unlockAt.HasValue)
            {
                result.TimestampExpired = DateTime.UtcNow >= unlockAt.Value;
                result.TimestampUnlockTime = unlockAt.Value;
            }

            // Check block-based timelock
            if (blockNumber.HasValue)
            {
                var currentBlock = await _ethereumService.GetCurrentBlockNumberAsync();
                result.BlockExpired = currentBlock >= blockNumber.Value;
                result.BlockUnlockNumber = blockNumber.Value;
                result.CurrentBlockNumber = currentBlock;
            }

            // Determine overall expiration
            if (unlockAt.HasValue && blockNumber.HasValue)
            {
                // Hybrid: both conditions must be met
                result.IsExpired = result.TimestampExpired && result.BlockExpired;
            }
            else if (unlockAt.HasValue)
            {
                // Timestamp only
                result.IsExpired = result.TimestampExpired;
            }
            else if (blockNumber.HasValue)
            {
                // Block only
                result.IsExpired = result.BlockExpired;
            }

            _logger.LogInformation("Timelock validation completed: {Result}", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating timelock");
            result.IsExpired = false; // Fail secure: don't allow access if validation fails
            return result;
        }
    }

    private async Task<byte[]> EncryptWithECDHAsync(byte[] data, ECDsa ecdsa)
    {
        // Simplified ECDH encryption for demo
        // In production, use proper ECDH key derivation
        using var tempEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        var sharedSecret = tempEcdsa.ExportParameters(true).D;
        
        using var aes = Aes.Create();
        aes.Key = sharedSecret.Take(32).ToArray();
        aes.GenerateIV();
        
        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(data, 0, data.Length);
    }

    private async Task<byte[]> DecryptWithECDHAsync(byte[] encryptedData, ECDsa ecdsa)
    {
        // Simplified ECDH decryption for demo
        // In production, use proper ECDH key derivation
        var privateKey = ecdsa.ExportParameters(true).D;
        
        using var aes = Aes.Create();
        aes.Key = privateKey.Take(32).ToArray();
        aes.IV = new byte[16]; // Use proper IV in production
        
        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
    }
}

public class TimelockKeyPair
{
    public string PublicKey { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}

public class TimelockValidationResult
{
    public bool IsExpired { get; set; }
    public DateTime ValidationTime { get; set; }
    
    // Timestamp validation
    public bool TimestampExpired { get; set; }
    public DateTime? TimestampUnlockTime { get; set; }
    
    // Block validation
    public bool BlockExpired { get; set; }
    public long? BlockUnlockNumber { get; set; }
    public long? CurrentBlockNumber { get; set; }
    
    public string? ErrorMessage { get; set; }
} 