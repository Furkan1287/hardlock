using System.ComponentModel.DataAnnotations;

namespace HardLock.Audit.Models;

public class AuditLogRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string Action { get; set; } = string.Empty;
    
    [Required]
    public string ResourceType { get; set; } = string.Empty;
    
    public string? ResourceId { get; set; }
    
    public string? Description { get; set; }
    
    public string? IpAddress { get; set; }
    
    public string? UserAgent { get; set; }
    
    public string? Location { get; set; }
    
    public Dictionary<string, object>? Metadata { get; set; }
    
    public string? Severity { get; set; } = "info"; // info, warning, error, critical
    
    public string? Service { get; set; }
}

public class AuditLogResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string? ResourceId { get; set; }
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Location { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string? Service { get; set; }
    public DateTime Timestamp { get; set; }
}

public class AuditSearchRequest
{
    public string? UserId { get; set; }
    public string? Action { get; set; }
    public string? ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public string? Severity { get; set; }
    public string? Service { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class AuditSearchResponse
{
    public List<AuditLogResponse> Logs { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class SecurityAlertRequest
{
    [Required]
    public string AlertType { get; set; } = string.Empty;
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public string Severity { get; set; } = string.Empty; // low, medium, high, critical
    
    public string? UserId { get; set; }
    
    public string? ResourceId { get; set; }
    
    public string? IpAddress { get; set; }
    
    public Dictionary<string, object>? Context { get; set; }
}

public class SecurityAlertResponse
{
    public string Id { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? ResourceId { get; set; }
    public string? IpAddress { get; set; }
    public Dictionary<string, object>? Context { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class AuditReportRequest
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string? UserId { get; set; }
    public string? Service { get; set; }
    public string ReportType { get; set; } = "summary"; // summary, detailed, security
}

public class AuditReportResponse
{
    public string ReportId { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public Dictionary<string, int> ActionCounts { get; set; } = new();
    public Dictionary<string, int> SeverityCounts { get; set; } = new();
    public Dictionary<string, int> ServiceCounts { get; set; } = new();
    public List<SecurityAlertResponse> SecurityAlerts { get; set; } = new();
    public int TotalLogs { get; set; }
    public int UniqueUsers { get; set; }
    public string ReportData { get; set; } = string.Empty; // JSON or CSV data
} 