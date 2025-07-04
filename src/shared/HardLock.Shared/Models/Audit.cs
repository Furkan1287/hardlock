using System.ComponentModel.DataAnnotations;

namespace HardLock.Shared.Models;

public class AuditLog
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string? ResourceId { get; set; }
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Location { get; set; }
    public string? Metadata { get; set; } // JSON string
    public string Severity { get; set; } = string.Empty;
    public string? Service { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class SecurityAlert
{
    public Guid Id { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? ResourceId { get; set; }
    public string? IpAddress { get; set; }
    public string? Context { get; set; } // JSON string
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsResolved { get; set; } = false;
    public DateTime? ResolvedAt { get; set; }
}

public class SecurityEvent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    
    public SecurityEventType EventType { get; set; }
    public SecuritySeverity Severity { get; set; } = SecuritySeverity.Medium;
    
    public string? FileId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public GeoLocation? Location { get; set; }
    
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string> Details { get; set; } = new();
    
    public bool RequiresAction { get; set; } = false;
    public SecurityAction? RecommendedAction { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsResolved { get; set; } = false;
    public DateTime? ResolvedAt { get; set; }
}

public class AccessAttempt
{
    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public GeoLocation? Location { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public enum AuditSeverity
{
    Debug,
    Info,
    Warning,
    Error,
    Critical
}

public enum AuditStatus
{
    Success,
    Failure,
    Partial
}

public enum SecurityEventType
{
    FailedLogin,
    FailedFileAccess,
    UnauthorizedAccess,
    SelfDestructTriggered,
    GeographicViolation,
    BruteForceAttempt,
    AnomalousBehavior,
    FileTampering,
    SystemIntrusion
}

public enum SecuritySeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum SecurityAction
{
    BlockIp,
    LockAccount,
    RequireMfa,
    NotifyAdmin,
    SelfDestructFile,
    QuarantineUser
} 