using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using HardLock.Shared.Models;

namespace HardLock.Security.Encryption;

public interface IFileEncryptionService
{
    Task<EncryptedData> EncryptFileAsync(byte[] fileData, string password, FileEncryptionOptions options);
    Task<byte[]> DecryptFileAsync(EncryptedData encryptedData, string password);
    Task<List<EncryptedShard>> ShardEncryptAsync(byte[] fileData, string password, int shardSize = 4096);
    Task<byte[]> ShardDecryptAsync(List<EncryptedShard> shards, string password);
    bool ValidatePassword(string password, EncryptedData encryptedData);
}

public class FileEncryptionService : IFileEncryptionService
{
    private readonly ILogger<FileEncryptionService> _logger;
    private readonly IKeyDerivationService _keyDerivationService;

    public FileEncryptionService(ILogger<FileEncryptionService> logger, IKeyDerivationService keyDerivationService)
    {
        _logger = logger;
        _keyDerivationService = keyDerivationService;
    }

    public async Task<EncryptedData> EncryptFileAsync(byte[] fileData, string password, FileEncryptionOptions options)
    {
        try
        {
            _logger.LogInformation("Starting file encryption with options: {@Options}", options);

            // Generate unique salt for this file
            var salt = GenerateSalt();
            
            // Derive encryption key from password and salt
            var key = await _keyDerivationService.DeriveKeyAsync(password, salt, options.KeyDerivationIterations);
            
            // Generate initialization vector
            var iv = GenerateIV();
            
            // Encrypt the file data
            var (ciphertext, tag) = await EncryptWithAesGcmAsync(fileData, key, iv);
            
            var encryptedData = new EncryptedData
            {
                CipherText = ciphertext,
                IV = iv,
                Salt = salt,
                Tag = tag,
                Algorithm = "AES-256-GCM",
                KeyDerivationAlgorithm = "PBKDF2-HMAC-SHA256",
                Iterations = options.KeyDerivationIterations,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("File encryption completed successfully. Size: {Size} bytes", fileData.Length);
            return encryptedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during file encryption");
            throw new CryptographicException("File encryption failed", ex);
        }
    }

    public async Task<byte[]> DecryptFileAsync(EncryptedData encryptedData, string password)
    {
        try
        {
            _logger.LogInformation("Starting file decryption");

            // Validate password first
            if (!ValidatePassword(password, encryptedData))
            {
                _logger.LogWarning("Password validation failed during decryption");
                throw new CryptographicException("Invalid password");
            }

            // Derive key from password and salt
            var key = await _keyDerivationService.DeriveKeyAsync(password, encryptedData.Salt, encryptedData.Iterations);
            
            // Decrypt the file data
            var decryptedData = await DecryptWithAesGcmAsync(encryptedData.CipherText, key, encryptedData.IV, encryptedData.Tag);
            
            _logger.LogInformation("File decryption completed successfully. Size: {Size} bytes", decryptedData.Length);
            return decryptedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during file decryption");
            throw new CryptographicException("File decryption failed", ex);
        }
    }

    public async Task<List<EncryptedShard>> ShardEncryptAsync(byte[] fileData, string password, int shardSize = 4096)
    {
        try
        {
            _logger.LogInformation("Starting sharded encryption with shard size: {ShardSize}", shardSize);

            var shards = new List<EncryptedShard>();
            var chunks = ChunkFile(fileData, shardSize);
            
            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                var shardPassword = $"{password}_{Guid.NewGuid()}"; // Unique password per shard
                
                var encryptedShard = await EncryptFileAsync(chunk, shardPassword, new FileEncryptionOptions());
                
                shards.Add(new EncryptedShard
                {
                    Id = Guid.NewGuid(),
                    Index = i,
                    EncryptedData = encryptedShard,
                    ShardHash = ComputeHash(chunk)
                });
            }

            _logger.LogInformation("Sharded encryption completed. Total shards: {ShardCount}", shards.Count);
            return shards;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sharded encryption");
            throw new CryptographicException("Sharded encryption failed", ex);
        }
    }

    public async Task<byte[]> ShardDecryptAsync(List<EncryptedShard> shards, string password)
    {
        try
        {
            _logger.LogInformation("Starting sharded decryption for {ShardCount} shards", shards.Count);

            var decryptedChunks = new List<byte[]>();
            
            foreach (var shard in shards.OrderBy(s => s.Index))
            {
                var shardPassword = $"{password}_{shard.Id}"; // Reconstruct shard password
                var decryptedChunk = await DecryptFileAsync(shard.EncryptedData, shardPassword);
                
                // Verify shard integrity
                var computedHash = ComputeHash(decryptedChunk);
                if (computedHash != shard.ShardHash)
                {
                    throw new CryptographicException($"Shard integrity check failed for shard {shard.Index}");
                }
                
                decryptedChunks.Add(decryptedChunk);
            }

            var reconstructedFile = CombineChunks(decryptedChunks);
            
            _logger.LogInformation("Sharded decryption completed successfully. Size: {Size} bytes", reconstructedFile.Length);
            return reconstructedFile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sharded decryption");
            throw new CryptographicException("Sharded decryption failed", ex);
        }
    }

    public bool ValidatePassword(string password, EncryptedData encryptedData)
    {
        try
        {
            // Create a small test data to validate password
            var testData = Encoding.UTF8.GetBytes("password_validation_test");
            var testSalt = GenerateSalt();
            var testKey = _keyDerivationService.DeriveKeyAsync(password, testSalt, 1000).Result;
            var testIv = GenerateIV();
            
            var (testCiphertext, testTag) = EncryptWithAesGcmAsync(testData, testKey, testIv).Result;
            var decryptedTest = DecryptWithAesGcmAsync(testCiphertext, testKey, testIv, testTag).Result;
            
            return testData.SequenceEqual(decryptedTest);
        }
        catch
        {
            return false;
        }
    }

    private async Task<(byte[] ciphertext, byte[] tag)> EncryptWithAesGcmAsync(byte[] plaintext, byte[] key, byte[] iv)
    {
        using var aes = new AesGcm(key);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];
        
        aes.Encrypt(iv, plaintext, ciphertext, tag);
        
        return await Task.FromResult((ciphertext, tag));
    }

    private async Task<byte[]> DecryptWithAesGcmAsync(byte[] ciphertext, byte[] key, byte[] iv, byte[] tag)
    {
        using var aes = new AesGcm(key);
        var plaintext = new byte[ciphertext.Length];
        
        aes.Decrypt(iv, ciphertext, tag, plaintext);
        
        return await Task.FromResult(plaintext);
    }

    private byte[] GenerateSalt()
    {
        var salt = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }

    private byte[] GenerateIV()
    {
        var iv = new byte[12]; // 96 bits for AES-GCM
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(iv);
        return iv;
    }

    private List<byte[]> ChunkFile(byte[] fileData, int chunkSize)
    {
        var chunks = new List<byte[]>();
        var offset = 0;
        
        while (offset < fileData.Length)
        {
            var remainingBytes = fileData.Length - offset;
            var currentChunkSize = Math.Min(chunkSize, remainingBytes);
            var chunk = new byte[currentChunkSize];
            
            Array.Copy(fileData, offset, chunk, 0, currentChunkSize);
            chunks.Add(chunk);
            
            offset += currentChunkSize;
        }
        
        return chunks;
    }

    private byte[] CombineChunks(List<byte[]> chunks)
    {
        var totalSize = chunks.Sum(chunk => chunk.Length);
        var combined = new byte[totalSize];
        var offset = 0;
        
        foreach (var chunk in chunks)
        {
            Array.Copy(chunk, 0, combined, offset, chunk.Length);
            offset += chunk.Length;
        }
        
        return combined;
    }

    private string ComputeHash(byte[] data)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(data);
        return Convert.ToBase64String(hash);
    }
}

public class EncryptedData
{
    public byte[] CipherText { get; set; } = Array.Empty<byte>();
    public byte[] IV { get; set; } = Array.Empty<byte>();
    public byte[] Salt { get; set; } = Array.Empty<byte>();
    public byte[] Tag { get; set; } = Array.Empty<byte>();
    public string Algorithm { get; set; } = string.Empty;
    public string KeyDerivationAlgorithm { get; set; } = string.Empty;
    public int Iterations { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EncryptedShard
{
    public Guid Id { get; set; }
    public int Index { get; set; }
    public EncryptedData EncryptedData { get; set; } = new();
    public string ShardHash { get; set; } = string.Empty;
}

public class FileEncryptionOptions
{
    public int KeyDerivationIterations { get; set; } = 100000;
    public bool EnableSharding { get; set; } = false;
    public int ShardSize { get; set; } = 4096;
    public bool EnableQuantumSafe { get; set; } = false;
} 