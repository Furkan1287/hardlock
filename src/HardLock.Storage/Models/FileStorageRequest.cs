using System.ComponentModel.DataAnnotations;

namespace HardLock.Storage.Models;

public class FileStorageRequest
{
    [Required]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    
    [Required]
    public string ContentType { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    
    public string? EncryptionKey { get; set; }
    
    public string? UserId { get; set; }
    
    public Dictionary<string, string>? Metadata { get; set; }
    
    public bool IsEncrypted { get; set; } = true;
    
    public string? StorageTier { get; set; } = "standard"; // standard, cold, archive
}

public class FileRetrievalRequest
{
    [Required]
    public string FileId { get; set; } = string.Empty;
    
    public string? UserId { get; set; }
    
    public string? EncryptionKey { get; set; }
}

public class FileStorageResponse
{
    public string FileId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string StorageLocation { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class FileRetrievalResponse
{
    public string FileId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime RetrievedAt { get; set; }
    public bool IsEncrypted { get; set; }
}

public class FileDeleteRequest
{
    [Required]
    public string FileId { get; set; } = string.Empty;
    
    public string? UserId { get; set; }
}

public class FileListRequest
{
    public string? UserId { get; set; }
    public string? FileType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class FileListResponse
{
    public List<FileStorageResponse> Files { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
} 