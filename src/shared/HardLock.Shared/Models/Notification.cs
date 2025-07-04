namespace HardLock.Shared.Models;

public class Notification
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Data { get; set; } // JSON string
    public bool IsUrgent { get; set; } = false;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public string Channels { get; set; } = string.Empty; // JSON array string
}

public class NotificationTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Variables { get; set; } // JSON string
    public string Channels { get; set; } = string.Empty; // JSON array string
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class NotificationPreferences
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool EmailEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; } = false;
    public bool PushEnabled { get; set; } = true;
    public bool InAppEnabled { get; set; } = true;
    public string EnabledCategories { get; set; } = string.Empty; // JSON array string
    public bool UrgentNotifications { get; set; } = true;
    public string? EmailAddress { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
} 