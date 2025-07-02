using System.ComponentModel.DataAnnotations;

namespace HardLock.Encryption.Models;

public class FileEncryptionRequest
{
    [Required]
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    
    [Required]
    public string Password { get; set; } = string.Empty;
    
    public string? FileName { get; set; }
    public int KeyDerivationIterations { get; set; } = 100000;
    public bool EnableSharding { get; set; } = false;
    public int ShardSize { get; set; } = 4096;
    public bool EnableQuantumSafe { get; set; } = false;
}

public class FileDecryptionRequest
{
    [Required]
    public EncryptedData EncryptedData { get; set; } = new();
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class ShardEncryptionRequest
{
    [Required]
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    
    [Required]
    public string Password { get; set; } = string.Empty;
    
    public int ShardSize { get; set; } = 4096;
}

public class ShardDecryptionRequest
{
    [Required]
    public List<EncryptedShard> Shards { get; set; } = new();
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class PasswordValidationRequest
{
    [Required]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    public EncryptedData EncryptedData { get; set; } = new();
}

public class EncryptionResponse
{
    public EncryptedData EncryptedData { get; set; } = new();
    public string FileHash { get; set; } = string.Empty;
    public long OriginalSize { get; set; }
    public long EncryptedSize { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

public class DecryptionResponse
{
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string FileHash { get; set; } = string.Empty;
    public TimeSpan ProcessingTime { get; set; }
}

public class ShardEncryptionResponse
{
    public List<EncryptedShard> Shards { get; set; } = new();
    public string FileHash { get; set; } = string.Empty;
    public int ShardCount { get; set; }
    public long OriginalSize { get; set; }
    public long TotalEncryptedSize { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

public class TimelockEncryptionRequest
{
    [Required]
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    
    [Required]
    public string Password { get; set; } = string.Empty;
    
    public DateTime? UnlockAt { get; set; }
    public long? BlockNumber { get; set; }
    public TimelockType TimelockType { get; set; } = TimelockType.Timestamp;
    public int KeyDerivationIterations { get; set; } = 100000;
    public bool EnableSharding { get; set; } = false;
    public int ShardSize { get; set; } = 4096;
    public bool EnableQuantumSafe { get; set; } = false;
}

public class TimelockDecryptionRequest
{
    [Required]
    public EncryptedData EncryptedData { get; set; } = new();
    
    [Required]
    public string TimelockPrivateKey { get; set; } = string.Empty;
    
    public DateTime? UnlockAt { get; set; }
    public long? BlockNumber { get; set; }
}

public class TimelockEncryptionResponse
{
    public EncryptedData EncryptedData { get; set; } = new();
    public string TimelockPublicKey { get; set; } = string.Empty;
    public string TimelockEncryptedKey { get; set; } = string.Empty;
    public DateTime? UnlockAt { get; set; }
    public long? BlockNumber { get; set; }
    public TimelockType TimelockType { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

// Darknet Backup Models
public class DarknetBackupRequest
{
    [Required]
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    
    [Required]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    public string EncryptionKey { get; set; } = string.Empty;
    
    public int ShardSize { get; set; } = 1024 * 1024; // 1MB
    public DateTime? ExpiresAt { get; set; }
    public int ReplicationFactor { get; set; } = 3;
    public bool EnableTorRouting { get; set; } = true;
}

public class DarknetRestoreRequest
{
    [Required]
    public string ContentHash { get; set; } = string.Empty;
    
    [Required]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    public string EncryptionKey { get; set; } = string.Empty;
}

// Geo-Fencing Models
public class GeoFencingValidationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? IPAddress { get; set; }
    public bool IsEnabled { get; set; }
    public List<string>? AllowedCountries { get; set; }
    public List<string>? AllowedCities { get; set; }
    public List<GeoRadius>? AllowedLocations { get; set; }
    public List<List<GeoLocation>>? AllowedPolygons { get; set; }
}

// File Hashing Models
public class FileHashRequest
{
    public string? FilePath { get; set; }
    public byte[]? FileData { get; set; }
    public string HashAlgorithm { get; set; } = "SHA256";
    public bool IsLargeFile { get; set; } = false;
    public int? BufferSize { get; set; }
}

public class FileHashResponse
{
    public bool Success { get; set; }
    public string Hash { get; set; } = string.Empty;
    public string Algorithm { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public long FileSize { get; set; }
    public DateTime HashTime { get; set; }
    public bool IsLargeFile { get; set; }
    public string? ErrorMessage { get; set; }
}

public class FileIntegrityRequest
{
    public string FilePath { get; set; } = string.Empty;
    public string ExpectedHash { get; set; } = string.Empty;
    public string HashAlgorithm { get; set; } = "SHA256";
}

public class FileIntegrityResponse
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

public class CreateIntegrityInfoRequest
{
    public string? FilePath { get; set; }
    public byte[]? FileData { get; set; }
    public string HashAlgorithm { get; set; } = "SHA256";
}

public class IntegrityInfoResponse
{
    public string Hash { get; set; } = string.Empty;
    public string Algorithm { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
} 