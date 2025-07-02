using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using HardLock.Encryption.Models;
using HardLock.Security.Encryption;

namespace HardLock.Encryption.Services;

public class EncryptionService : IEncryptionService
{
    private readonly IFileEncryptionService _fileEncryptionService;
    private readonly ITimelockService _timelockService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<EncryptionService> _logger;

    public EncryptionService(
        IFileEncryptionService fileEncryptionService,
        ITimelockService timelockService,
        IMemoryCache cache,
        ILogger<EncryptionService> logger)
    {
        _fileEncryptionService = fileEncryptionService;
        _timelockService = timelockService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<EncryptionResponse> EncryptFileAsync(FileEncryptionRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Starting file encryption. Size: {Size} bytes", request.FileData.Length);

            // Check cache for duplicate files
            var fileHash = ComputeFileHash(request.FileData);
            var cacheKey = $"encrypted:{fileHash}:{request.Password}";
            
            if (_cache.TryGetValue(cacheKey, out EncryptionResponse? cachedResponse))
            {
                _logger.LogInformation("Returning cached encryption result");
                return cachedResponse!;
            }

            var options = new FileEncryptionOptions
            {
                KeyDerivationIterations = request.KeyDerivationIterations,
                EnableSharding = request.EnableSharding,
                ShardSize = request.ShardSize,
                EnableQuantumSafe = request.EnableQuantumSafe
            };

            var encryptedData = await _fileEncryptionService.EncryptFileAsync(request.FileData, request.Password, options);

            stopwatch.Stop();

            var response = new EncryptionResponse
            {
                EncryptedData = encryptedData,
                FileHash = fileHash,
                OriginalSize = request.FileData.Length,
                EncryptedSize = encryptedData.CipherText.Length,
                ProcessingTime = stopwatch.Elapsed
            };

            // Cache the result for 1 hour
            _cache.Set(cacheKey, response, TimeSpan.FromHours(1));

            _logger.LogInformation("File encryption completed successfully. Processing time: {ProcessingTime}ms", stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error during file encryption. Processing time: {ProcessingTime}ms", stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<DecryptionResponse> DecryptFileAsync(FileDecryptionRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Starting file decryption");

            var decryptedData = await _fileEncryptionService.DecryptFileAsync(request.EncryptedData, request.Password);
            var fileHash = ComputeFileHash(decryptedData);

            stopwatch.Stop();

            var response = new DecryptionResponse
            {
                FileData = decryptedData,
                FileHash = fileHash,
                ProcessingTime = stopwatch.Elapsed
            };

            _logger.LogInformation("File decryption completed successfully. Processing time: {ProcessingTime}ms", stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error during file decryption. Processing time: {ProcessingTime}ms", stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<ShardEncryptionResponse> ShardEncryptAsync(ShardEncryptionRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Starting sharded encryption. Size: {Size} bytes, Shard size: {ShardSize}", 
                request.FileData.Length, request.ShardSize);

            var shards = await _fileEncryptionService.ShardEncryptAsync(request.FileData, request.Password, request.ShardSize);
            var fileHash = ComputeFileHash(request.FileData);

            stopwatch.Stop();

            var response = new ShardEncryptionResponse
            {
                Shards = shards,
                FileHash = fileHash,
                ShardCount = shards.Count,
                OriginalSize = request.FileData.Length,
                TotalEncryptedSize = shards.Sum(s => s.EncryptedData.CipherText.Length),
                ProcessingTime = stopwatch.Elapsed
            };

            _logger.LogInformation("Sharded encryption completed successfully. Shards: {ShardCount}, Processing time: {ProcessingTime}ms", 
                shards.Count, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error during sharded encryption. Processing time: {ProcessingTime}ms", stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<DecryptionResponse> ShardDecryptAsync(ShardDecryptionRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Starting sharded decryption. Shards: {ShardCount}", request.Shards.Count);

            var decryptedData = await _fileEncryptionService.ShardDecryptAsync(request.Shards, request.Password);
            var fileHash = ComputeFileHash(decryptedData);

            stopwatch.Stop();

            var response = new DecryptionResponse
            {
                FileData = decryptedData,
                FileHash = fileHash,
                ProcessingTime = stopwatch.Elapsed
            };

            _logger.LogInformation("Sharded decryption completed successfully. Processing time: {ProcessingTime}ms", stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error during sharded decryption. Processing time: {ProcessingTime}ms", stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<bool> ValidatePasswordAsync(PasswordValidationRequest request)
    {
        try
        {
            _logger.LogInformation("Validating password for encrypted data");

            var isValid = _fileEncryptionService.ValidatePassword(request.Password, request.EncryptedData);

            _logger.LogInformation("Password validation completed: {IsValid}", isValid);

            return await Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password validation");
            return false;
        }
    }

    public async Task<TimelockEncryptionResponse> EncryptWithTimelockAsync(TimelockEncryptionRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Starting timelock encryption. Unlock at: {UnlockAt}, Block: {BlockNumber}", 
                request.UnlockAt, request.BlockNumber);

            // Generate timelock key pair
            var timelockKeys = await _timelockService.GenerateTimelockKeysAsync();
            
            // Encrypt the file normally first
            var options = new FileEncryptionOptions
            {
                KeyDerivationIterations = request.KeyDerivationIterations,
                EnableSharding = request.EnableSharding,
                ShardSize = request.ShardSize,
                EnableQuantumSafe = request.EnableQuantumSafe
            };

            var encryptedData = await _fileEncryptionService.EncryptFileAsync(request.FileData, request.Password, options);
            
            // Encrypt the file key with timelock
            var encryptedFileKey = await _timelockService.EncryptFileKeyAsync(encryptedData.Key, timelockKeys.PublicKey);

            stopwatch.Stop();

            var response = new TimelockEncryptionResponse
            {
                EncryptedData = encryptedData,
                TimelockPublicKey = timelockKeys.PublicKey,
                TimelockEncryptedKey = encryptedFileKey,
                UnlockAt = request.UnlockAt,
                BlockNumber = request.BlockNumber,
                TimelockType = request.TimelockType,
                ProcessingTime = stopwatch.Elapsed
            };

            _logger.LogInformation("Timelock encryption completed successfully. Processing time: {ProcessingTime}ms", 
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error during timelock encryption. Processing time: {ProcessingTime}ms", 
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<DecryptionResponse> DecryptWithTimelockAsync(TimelockDecryptionRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Starting timelock decryption");

            // Validate timelock
            var isExpired = await _timelockService.IsTimelockExpiredAsync(request.UnlockAt, request.BlockNumber);
            
            if (!isExpired)
            {
                var validation = await _timelockService.ValidateTimelockAsync(request.UnlockAt, request.BlockNumber);
                throw new InvalidOperationException($"Timelock not expired. Unlock at: {validation.TimestampUnlockTime}, Block: {validation.BlockUnlockNumber}");
            }

            // Decrypt the file key with timelock
            var decryptedFileKey = await _timelockService.DecryptFileKeyAsync(request.TimelockEncryptedKey, request.TimelockPrivateKey);
            
            if (string.IsNullOrEmpty(decryptedFileKey))
            {
                throw new InvalidOperationException("Failed to decrypt file key with timelock");
            }

            // Decrypt the file with the decrypted key
            var decryptedData = await _fileEncryptionService.DecryptFileAsync(request.EncryptedData, decryptedFileKey);
            var fileHash = ComputeFileHash(decryptedData);

            stopwatch.Stop();

            var response = new DecryptionResponse
            {
                FileData = decryptedData,
                FileHash = fileHash,
                ProcessingTime = stopwatch.Elapsed
            };

            _logger.LogInformation("Timelock decryption completed successfully. Processing time: {ProcessingTime}ms", 
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error during timelock decryption. Processing time: {ProcessingTime}ms", 
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private static string ComputeFileHash(byte[] data)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(data);
        return Convert.ToBase64String(hash);
    }
} 