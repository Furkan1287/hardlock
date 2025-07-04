namespace HardLock.Shared.Models;

public class FileMetadata
{
    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 