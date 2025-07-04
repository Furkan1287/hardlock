using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace HardLock.Security.Encryption;

public interface IFileHashingService
{
    Task<FileHashResult> HashFileAsync(byte[] fileData, HashAlgorithm algorithm = HashAlgorithm.SHA256);
    Task<FileHashResult> HashFileAsync(string filePath, HashAlgorithm algorithm = HashAlgorithm.SHA256);
    Task<FileHashResult> HashLargeFileAsync(string filePath, HashAlgorithm algorithm = HashAlgorithm.SHA256, int bufferSize = 8192);
    Task<bool> VerifyFileHashAsync(byte[] fileData, string expectedHash, HashAlgorithm algorithm = HashAlgorithm.SHA256);
    Task<bool> VerifyFileHashAsync(string filePath, string expectedHash, HashAlgorithm algorithm = HashAlgorithm.SHA256);
    Task<FileIntegrityResult> VerifyFileIntegrityAsync(string filePath, FileIntegrityInfo integrityInfo);
    Task<FileIntegrityInfo> CreateIntegrityInfoAsync(byte[] fileData, HashAlgorithm algorithm = HashAlgorithm.SHA256);
    Task<FileIntegrityInfo> CreateIntegrityInfoAsync(string filePath, HashAlgorithm algorithm = HashAlgorithm.SHA256);
}

public class FileHashingService : IFileHashingService
{
    private readonly ILogger<FileHashingService> _logger;

    public FileHashingService(ILogger<FileHashingService> logger)
    {
        _logger = logger;
    }

    public async Task<FileHashResult> HashFileAsync(byte[] fileData, HashAlgorithm algorithm = HashAlgorithm.SHA256)
    {
        try
        {
            _logger.LogInformation("Hashing file data with {Algorithm}", algorithm);

            var hash = await ComputeHashAsync(fileData, algorithm);
            var result = new FileHashResult
            {
                Hash = hash,
                Algorithm = algorithm,
                FileSize = fileData.Length,
                HashTime = DateTime.UtcNow,
                Success = true
            };

            _logger.LogInformation("File hashed successfully. Hash: {Hash}", hash);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hashing file data");
            return new FileHashResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<FileHashResult> HashFileAsync(string filePath, HashAlgorithm algorithm = HashAlgorithm.SHA256)
    {
        try
        {
            _logger.LogInformation("Hashing file: {FilePath} with {Algorithm}", filePath, algorithm);

            if (!System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var fileInfo = new FileInfo(filePath);
            var fileData = await System.IO.File.ReadAllBytesAsync(filePath);
            
            var hash = await ComputeHashAsync(fileData, algorithm);
            var result = new FileHashResult
            {
                Hash = hash,
                Algorithm = algorithm,
                FilePath = filePath,
                FileSize = fileInfo.Length,
                HashTime = DateTime.UtcNow,
                Success = true
            };

            _logger.LogInformation("File hashed successfully. Hash: {Hash}", hash);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hashing file: {FilePath}", filePath);
            return new FileHashResult
            {
                FilePath = filePath,
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<FileHashResult> HashLargeFileAsync(string filePath, HashAlgorithm algorithm = HashAlgorithm.SHA256, int bufferSize = 8192)
    {
        try
        {
            _logger.LogInformation("Hashing large file: {FilePath} with {Algorithm}", filePath, algorithm);

            if (!System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var fileInfo = new FileInfo(filePath);
            var hash = await ComputeHashStreamAsync(filePath, algorithm, bufferSize);
            
            var result = new FileHashResult
            {
                Hash = hash,
                Algorithm = algorithm,
                FilePath = filePath,
                FileSize = fileInfo.Length,
                HashTime = DateTime.UtcNow,
                Success = true,
                IsLargeFile = true
            };

            _logger.LogInformation("Large file hashed successfully. Hash: {Hash}", hash);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hashing large file: {FilePath}", filePath);
            return new FileHashResult
            {
                FilePath = filePath,
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<bool> VerifyFileHashAsync(byte[] fileData, string expectedHash, HashAlgorithm algorithm = HashAlgorithm.SHA256)
    {
        try
        {
            _logger.LogInformation("Verifying file hash with {Algorithm}", algorithm);

            var actualHash = await ComputeHashAsync(fileData, algorithm);
            var isValid = string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);

            _logger.LogInformation("File hash verification: {IsValid}", isValid);
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying file hash");
            return false;
        }
    }

    public async Task<bool> VerifyFileHashAsync(string filePath, string expectedHash, HashAlgorithm algorithm = HashAlgorithm.SHA256)
    {
        try
        {
            _logger.LogInformation("Verifying file hash: {FilePath} with {Algorithm}", filePath, algorithm);

            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning("File not found: {FilePath}", filePath);
                return false;
            }

            var actualHash = await ComputeHashStreamAsync(filePath, algorithm);
            var isValid = string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);

            _logger.LogInformation("File hash verification: {IsValid}", isValid);
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying file hash: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<FileIntegrityResult> VerifyFileIntegrityAsync(string filePath, FileIntegrityInfo integrityInfo)
    {
        try
        {
            _logger.LogInformation("Verifying file integrity: {FilePath}", filePath);

            if (!System.IO.File.Exists(filePath))
            {
                return new FileIntegrityResult
                {
                    IsValid = false,
                    Reason = "File not found",
                    FilePath = filePath
                };
            }

            var fileInfo = new FileInfo(filePath);
            
            // Check file size
            if (fileInfo.Length != integrityInfo.FileSize)
            {
                return new FileIntegrityResult
                {
                    IsValid = false,
                    Reason = "File size mismatch",
                    FilePath = filePath,
                    ExpectedSize = integrityInfo.FileSize,
                    ActualSize = fileInfo.Length
                };
            }

            // Check file hash
            var actualHash = await ComputeHashStreamAsync(filePath, integrityInfo.Algorithm);
            var hashValid = string.Equals(actualHash, integrityInfo.Hash, StringComparison.OrdinalIgnoreCase);

            if (!hashValid)
            {
                return new FileIntegrityResult
                {
                    IsValid = false,
                    Reason = "File hash mismatch",
                    FilePath = filePath,
                    ExpectedHash = integrityInfo.Hash,
                    ActualHash = actualHash
                };
            }

            // Check file modification time (optional)
            if (integrityInfo.CreatedAt.HasValue && fileInfo.CreationTimeUtc > integrityInfo.CreatedAt.Value.AddMinutes(5))
            {
                _logger.LogWarning("File creation time is newer than expected: {FilePath}", filePath);
            }

            return new FileIntegrityResult
            {
                IsValid = true,
                Reason = "File integrity verified",
                FilePath = filePath,
                VerificationTime = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying file integrity: {FilePath}", filePath);
            return new FileIntegrityResult
            {
                IsValid = false,
                Reason = "Verification error",
                FilePath = filePath,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<FileIntegrityInfo> CreateIntegrityInfoAsync(byte[] fileData, HashAlgorithm algorithm = HashAlgorithm.SHA256)
    {
        try
        {
            var hash = await ComputeHashAsync(fileData, algorithm);
            
            return new FileIntegrityInfo
            {
                Hash = hash,
                Algorithm = algorithm,
                FileSize = fileData.Length,
                CreatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating integrity info");
            throw;
        }
    }

    public async Task<FileIntegrityInfo> CreateIntegrityInfoAsync(string filePath, HashAlgorithm algorithm = HashAlgorithm.SHA256)
    {
        try
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var fileInfo = new FileInfo(filePath);
            var hash = await ComputeHashStreamAsync(filePath, algorithm);
            
            return new FileIntegrityInfo
            {
                Hash = hash,
                Algorithm = algorithm,
                FilePath = filePath,
                FileSize = fileInfo.Length,
                CreatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating integrity info for file: {FilePath}", filePath);
            throw;
        }
    }

    private async Task<string> ComputeHashAsync(byte[] data, HashAlgorithm algorithm)
    {
        using var hashAlgorithm = GetHashAlgorithm(algorithm);
        var hashBytes = await hashAlgorithm.ComputeHashAsync(new MemoryStream(data));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private async Task<string> ComputeHashStreamAsync(string filePath, HashAlgorithm algorithm, int bufferSize = 8192)
    {
        using var hashAlgorithm = GetHashAlgorithm(algorithm);
        using var fileStream = System.IO.File.OpenRead(filePath);
        
        var buffer = new byte[bufferSize];
        int bytesRead;
        
        while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            hashAlgorithm.TransformBlock(buffer, 0, bytesRead, null, 0);
        }
        
        hashAlgorithm.TransformFinalBlock(buffer, 0, 0);
        var hashBytes = hashAlgorithm.Hash;
        
        return Convert.ToHexString(hashBytes!).ToLowerInvariant();
    }

    private System.Security.Cryptography.HashAlgorithm GetHashAlgorithm(HashAlgorithm algorithm)
    {
        return algorithm switch
        {
            HashAlgorithm.MD5 => MD5.Create(),
            HashAlgorithm.SHA1 => SHA1.Create(),
            HashAlgorithm.SHA256 => SHA256.Create(),
            HashAlgorithm.SHA384 => SHA384.Create(),
            HashAlgorithm.SHA512 => SHA512.Create(),
            _ => SHA256.Create()
        };
    }
}

public enum HashAlgorithm
{
    MD5,
    SHA1,
    SHA256,
    SHA384,
    SHA512
}

public class FileHashResult
{
    public bool Success { get; set; }
    public string Hash { get; set; } = string.Empty;
    public HashAlgorithm Algorithm { get; set; }
    public string? FilePath { get; set; }
    public long FileSize { get; set; }
    public DateTime HashTime { get; set; }
    public bool IsLargeFile { get; set; }
    public string? ErrorMessage { get; set; }
}

public class FileIntegrityInfo
{
    public string Hash { get; set; } = string.Empty;
    public HashAlgorithm Algorithm { get; set; }
    public string? FilePath { get; set; }
    public long FileSize { get; set; }
    public DateTime? CreatedAt { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class FileIntegrityResult
{
    public bool IsValid { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? ExpectedHash { get; set; }
    public string? ActualHash { get; set; }
    public long? ExpectedSize { get; set; }
    public long? ActualSize { get; set; }
    public DateTime? VerificationTime { get; set; }
    public string? ErrorMessage { get; set; }
} 