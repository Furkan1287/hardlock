using System.ComponentModel.DataAnnotations;

namespace HardLock.Notification.Models;

public class NotificationRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string? Message { get; set; }
    
    [Required]
    public string Type { get; set; } = string.Empty; // info, warning, error, success
    
    public string? Category { get; set; } // security, system, user, file
    
    public Dictionary<string, object>? Data { get; set; }
    
    public bool IsUrgent { get; set; } = false;
    
    public DateTime? ExpiresAt { get; set; }
    
    public List<string>? Channels { get; set; } = new() { "in-app" }; // in-app, email, sms, push
}

public class NotificationResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Category { get; set; }
    public Dictionary<string, object>? Data { get; set; }
    public bool IsUrgent { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<string> Channels { get; set; } = new();
}

public class NotificationTemplateRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string? Message { get; set; }
    
    [Required]
    public string Type { get; set; } = string.Empty;
    
    public string? Category { get; set; }
    
    public Dictionary<string, string>? Variables { get; set; }
    
    public List<string>? Channels { get; set; }
}

public class NotificationTemplateResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Category { get; set; }
    public Dictionary<string, string>? Variables { get; set; }
    public List<string> Channels { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class NotificationPreferencesRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public bool EmailEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; } = false;
    public bool PushEnabled { get; set; } = true;
    public bool InAppEnabled { get; set; } = true;
    
    public List<string>? EnabledCategories { get; set; }
    
    public bool UrgentNotifications { get; set; } = true;
    
    public string? EmailAddress { get; set; }
    public string? PhoneNumber { get; set; }
}

public class NotificationPreferencesResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }
    public bool PushEnabled { get; set; }
    public bool InAppEnabled { get; set; }
    public List<string> EnabledCategories { get; set; } = new();
    public bool UrgentNotifications { get; set; }
    public string? EmailAddress { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class NotificationSearchRequest
{
    public string? UserId { get; set; }
    public string? Type { get; set; }
    public string? Category { get; set; }
    public bool? IsRead { get; set; }
    public bool? IsUrgent { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class NotificationSearchResponse
{
    public List<NotificationResponse> Notifications { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int UnreadCount { get; set; }
}

public class BulkNotificationRequest
{
    [Required]
    public List<string> UserIds { get; set; } = new();
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string? Message { get; set; }
    
    [Required]
    public string Type { get; set; } = string.Empty;
    
    public string? Category { get; set; }
    
    public Dictionary<string, object>? Data { get; set; }
    
    public bool IsUrgent { get; set; } = false;
    
    public DateTime? ExpiresAt { get; set; }
    
    public List<string>? Channels { get; set; }
}

public class BulkNotificationResponse
{
    public int TotalSent { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> FailedUserIds { get; set; } = new();
    public List<string> NotificationIds { get; set; } = new();
} 