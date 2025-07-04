using System.ComponentModel.DataAnnotations;

namespace HardLock.Shared.Models;

public class File
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public bool IsEncrypted { get; set; }
    public string StorageTier { get; set; } = "hot";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class FileAccess
{
    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public Guid UserId { get; set; }
    public FilePermission Permission { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class FileUploadRequest
{
    [Required]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? UnlockAt { get; set; }
    public bool EnableSharding { get; set; } = false;
    public bool EnableTimelock { get; set; } = false;
    public TimelockType TimelockType { get; set; } = TimelockType.None;
    public long? TimelockBlockNumber { get; set; }
    public bool EnableSelfDestruct { get; set; } = true;
    public int MaxAccessAttempts { get; set; } = 3;
    public FileAccessLevel AccessLevel { get; set; } = FileAccessLevel.Private;
    public GeoLocation? AllowedLocation { get; set; }
    public double? AllowedRadius { get; set; }
}

public class FileDownloadRequest
{
    [Required]
    public string Password { get; set; } = string.Empty;
    
    public string? DeviceId { get; set; }
    public GeoLocation? CurrentLocation { get; set; }
    public string? BiometricData { get; set; }
}

public class FileShareRequest
{
    [Required]
    public string TargetUserEmail { get; set; } = string.Empty;
    
    [Required]
    public FilePermission Permission { get; set; }
    
    public DateTime? ExpiresAt { get; set; }
}

public class GeoLocation
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public enum FileStatus
{
    Active,
    Deleted,
    SelfDestructed,
    Locked,
    Archived
}

public enum FileAccessLevel
{
    Private,
    Shared,
    Public
}

public enum FilePermission
{
    Read,
    Write,
    Admin
}

public enum TimelockType
{
    None,
    Timestamp,      // Unlock at specific date/time
    BlockNumber,    // Unlock at specific Ethereum block
    Hybrid          // Both timestamp and block number
} 