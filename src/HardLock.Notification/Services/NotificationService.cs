using HardLock.Notification.Models;
using HardLock.Notification.Data;
using HardLock.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HardLock.Notification.Services;

public class NotificationService : INotificationService
{
    private readonly NotificationDbContext _context;
    private readonly Microsoft.Extensions.Logging.ILogger<NotificationService> _logger;

    public NotificationService(NotificationDbContext context, Microsoft.Extensions.Logging.ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<NotificationResponse> SendNotificationAsync(NotificationRequest request)
    {
        try
        {
            var notification = new HardLock.Shared.Models.Notification
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Title = request.Title,
                Message = request.Message,
                Type = request.Type,
                Category = request.Category,
                Data = request.Data != null ? JsonSerializer.Serialize(request.Data) : null,
                IsUrgent = request.IsUrgent,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                Channels = request.Channels != null ? JsonSerializer.Serialize(request.Channels) : JsonSerializer.Serialize(new List<string> { "in-app" })
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send real-time notification
            await SendRealTimeNotificationAsync(request.UserId, new NotificationResponse
            {
                Id = notification.Id.ToString(),
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                Category = notification.Category,
                Data = notification.Data != null ? JsonSerializer.Deserialize<Dictionary<string, object>>(notification.Data) : null,
                IsUrgent = notification.IsUrgent,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                Channels = notification.Channels != null ? JsonSerializer.Deserialize<List<string>>(notification.Channels) ?? new List<string>() : new List<string>()
            });

            _logger.LogInformation("Notification sent: {UserId} - {Title} - {Type}", 
                request.UserId, request.Title, request.Type);

            return new NotificationResponse
            {
                Id = notification.Id.ToString(),
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                Category = notification.Category,
                Data = notification.Data != null ? JsonSerializer.Deserialize<Dictionary<string, object>>(notification.Data) : null,
                IsUrgent = notification.IsUrgent,
                IsRead = notification.IsRead,
                Channels = notification.Channels != null ? JsonSerializer.Deserialize<List<string>>(notification.Channels) ?? new List<string>() : new List<string>(),
                CreatedAt = notification.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification");
            throw;
        }
    }

    public async Task<BulkNotificationResponse> SendBulkNotificationAsync(BulkNotificationRequest request)
    {
        try
        {
            var notifications = new List<HardLock.Shared.Models.Notification>();
            var notificationIds = new List<string>();
            var failedUserIds = new List<string>();

            foreach (var userId in request.UserIds)
            {
                try
                {
                    var notification = new HardLock.Shared.Models.Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Title = request.Title,
                        Message = request.Message,
                        Type = request.Type,
                        Category = request.Category,
                        Data = request.Data != null ? JsonSerializer.Serialize(request.Data) : null,
                        IsUrgent = request.IsUrgent,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow,
                        Channels = request.Channels != null ? JsonSerializer.Serialize(request.Channels) : JsonSerializer.Serialize(new List<string> { "in-app" })
                    };

                    notifications.Add(notification);
                    notificationIds.Add(notification.Id.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating notification for user: {UserId}", userId);
                    failedUserIds.Add(userId);
                }
            }

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Bulk notification sent: {TotalSent} notifications to {UserCount} users", 
                notifications.Count, request.UserIds.Count);

            return new BulkNotificationResponse
            {
                TotalSent = request.UserIds.Count,
                SuccessCount = notifications.Count,
                FailureCount = failedUserIds.Count,
                FailedUserIds = failedUserIds,
                NotificationIds = notificationIds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk notification");
            throw;
        }
    }

    public async Task<bool> MarkAsReadAsync(string notificationId, string userId)
    {
        try
        {
            if (!Guid.TryParse(notificationId, out var id))
            {
                return false;
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
            {
                return false;
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification marked as read: {NotificationId}", notificationId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read: {NotificationId}", notificationId);
            return false;
        }
    }

    public async Task<bool> MarkAllAsReadAsync(string userId)
    {
        try
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("All notifications marked as read for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> DeleteNotificationAsync(string notificationId, string userId)
    {
        try
        {
            if (!Guid.TryParse(notificationId, out var id))
            {
                return false;
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
            {
                return false;
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification deleted: {NotificationId}", notificationId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification: {NotificationId}", notificationId);
            return false;
        }
    }

    public async Task<NotificationSearchResponse> GetNotificationsAsync(NotificationSearchRequest request)
    {
        try
        {
            var query = _context.Notifications.AsQueryable();

            if (!string.IsNullOrEmpty(request.UserId))
            {
                query = query.Where(n => n.UserId == request.UserId);
            }

            if (!string.IsNullOrEmpty(request.Type))
            {
                query = query.Where(n => n.Type == request.Type);
            }

            if (!string.IsNullOrEmpty(request.Category))
            {
                query = query.Where(n => n.Category == request.Category);
            }

            if (request.IsRead.HasValue)
            {
                query = query.Where(n => n.IsRead == request.IsRead.Value);
            }

            if (request.IsUrgent.HasValue)
            {
                query = query.Where(n => n.IsUrgent == request.IsUrgent.Value);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(n => n.CreatedAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(n => n.CreatedAt <= request.ToDate.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            var unreadCount = await query.CountAsync(n => !n.IsRead);

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(n => new NotificationResponse
                {
                    Id = n.Id.ToString(),
                    UserId = n.UserId,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    Category = n.Category,
                    Data = null, // Will be deserialized after query
                    IsUrgent = n.IsUrgent,
                    IsRead = n.IsRead,
                    Channels = null, // Will be deserialized after query
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            // Deserialize data and channels after query
            foreach (var notification in notifications)
            {
                var originalNotification = await _context.Notifications.FindAsync(Guid.Parse(notification.Id));
                if (originalNotification?.Data != null)
                {
                    notification.Data = JsonSerializer.Deserialize<Dictionary<string, object>>(originalNotification.Data);
                }
                if (originalNotification?.Channels != null)
                {
                    notification.Channels = JsonSerializer.Deserialize<List<string>>(originalNotification.Channels) ?? new List<string>();
                }
            }

            return new NotificationSearchResponse
            {
                Notifications = notifications,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                UnreadCount = unreadCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications");
            throw;
        }
    }

    public async Task<NotificationResponse?> GetNotificationAsync(string notificationId, string userId)
    {
        try
        {
            if (!Guid.TryParse(notificationId, out var id))
            {
                return null;
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
            {
                return null;
            }

            return new NotificationResponse
            {
                Id = notification.Id.ToString(),
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                Category = notification.Category,
                Data = notification.Data != null ? JsonSerializer.Deserialize<Dictionary<string, object>>(notification.Data) : null,
                IsUrgent = notification.IsUrgent,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ExpiresAt = notification.ExpiresAt,
                Channels = notification.Channels != null ? JsonSerializer.Deserialize<List<string>>(notification.Channels) ?? new List<string>() : new List<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification: {NotificationId}", notificationId);
            return null;
        }
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        try
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count for user: {UserId}", userId);
            return 0;
        }
    }

    public async Task<NotificationTemplateResponse> CreateTemplateAsync(NotificationTemplateRequest request)
    {
        try
        {
            var template = new NotificationTemplate
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Title = request.Title,
                Message = request.Message,
                Type = request.Type,
                Category = request.Category,
                Variables = request.Variables != null ? JsonSerializer.Serialize(request.Variables) : null,
                Channels = request.Channels != null ? JsonSerializer.Serialize(request.Channels) : JsonSerializer.Serialize(new List<string> { "in-app" }),
                CreatedAt = DateTime.UtcNow
            };

            _context.NotificationTemplates.Add(template);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification template created: {TemplateName}", request.Name);

            return new NotificationTemplateResponse
            {
                Id = template.Id.ToString(),
                Name = template.Name,
                Title = template.Title,
                Message = template.Message,
                Type = template.Type,
                Category = template.Category,
                Variables = template.Variables != null ? JsonSerializer.Deserialize<Dictionary<string, string>>(template.Variables) : null,
                Channels = template.Channels != null ? JsonSerializer.Deserialize<List<string>>(template.Channels) ?? new List<string>() : new List<string>(),
                CreatedAt = template.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification template");
            throw;
        }
    }

    public async Task<NotificationTemplateResponse> UpdateTemplateAsync(string templateId, NotificationTemplateRequest request)
    {
        try
        {
            if (!Guid.TryParse(templateId, out var id))
            {
                throw new ArgumentException("Invalid template ID");
            }

            var template = await _context.NotificationTemplates.FindAsync(id);
            if (template == null)
            {
                throw new KeyNotFoundException("Template not found");
            }

            template.Name = request.Name;
            template.Title = request.Title;
            template.Message = request.Message;
            template.Type = request.Type;
            template.Category = request.Category;
            template.Variables = request.Variables != null ? JsonSerializer.Serialize(request.Variables) : null;
            template.Channels = request.Channels != null ? JsonSerializer.Serialize(request.Channels) : JsonSerializer.Serialize(new List<string> { "in-app" });
            template.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification template updated: {TemplateName}", request.Name);

            return new NotificationTemplateResponse
            {
                Id = template.Id.ToString(),
                Name = template.Name,
                Title = template.Title,
                Message = template.Message,
                Type = template.Type,
                Category = template.Category,
                Variables = template.Variables != null ? JsonSerializer.Deserialize<Dictionary<string, string>>(template.Variables) : null,
                Channels = template.Channels != null ? JsonSerializer.Deserialize<List<string>>(template.Channels) ?? new List<string>() : new List<string>(),
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification template: {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<bool> DeleteTemplateAsync(string templateId)
    {
        try
        {
            if (!Guid.TryParse(templateId, out var id))
            {
                return false;
            }

            var template = await _context.NotificationTemplates.FindAsync(id);
            if (template == null)
            {
                return false;
            }

            _context.NotificationTemplates.Remove(template);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification template deleted: {TemplateId}", templateId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification template: {TemplateId}", templateId);
            return false;
        }
    }

    public async Task<List<NotificationTemplateResponse>> GetTemplatesAsync()
    {
        try
        {
            var templates = await _context.NotificationTemplates.ToListAsync();

            return templates.Select(t => new NotificationTemplateResponse
            {
                Id = t.Id.ToString(),
                Name = t.Name,
                Title = t.Title,
                Message = t.Message,
                Type = t.Type,
                Category = t.Category,
                Variables = t.Variables != null ? JsonSerializer.Deserialize<Dictionary<string, string>>(t.Variables) : null,
                Channels = t.Channels != null ? JsonSerializer.Deserialize<List<string>>(t.Channels) ?? new List<string>() : new List<string>(),
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification templates");
            throw;
        }
    }

    public async Task<NotificationTemplateResponse?> GetTemplateAsync(string templateId)
    {
        try
        {
            if (!Guid.TryParse(templateId, out var id))
            {
                return null;
            }

            var template = await _context.NotificationTemplates.FindAsync(id);
            if (template == null)
            {
                return null;
            }

            return new NotificationTemplateResponse
            {
                Id = template.Id.ToString(),
                Name = template.Name,
                Title = template.Title,
                Message = template.Message,
                Type = template.Type,
                Category = template.Category,
                Variables = template.Variables != null ? JsonSerializer.Deserialize<Dictionary<string, string>>(template.Variables) : null,
                Channels = template.Channels != null ? JsonSerializer.Deserialize<List<string>>(template.Channels) ?? new List<string>() : new List<string>(),
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification template: {TemplateId}", templateId);
            return null;
        }
    }

    public async Task<NotificationResponse> SendFromTemplateAsync(string templateName, string userId, Dictionary<string, string>? variables = null)
    {
        try
        {
            var template = await _context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.Name == templateName);

            if (template == null)
            {
                throw new KeyNotFoundException($"Template '{templateName}' not found");
            }

            var title = template.Title;
            var message = template.Message;

            if (variables != null && template.Variables != null)
            {
                var templateVariables = JsonSerializer.Deserialize<Dictionary<string, string>>(template.Variables);
                if (templateVariables != null)
                {
                    foreach (var variable in variables)
                    {
                        title = title?.Replace($"{{{variable.Key}}}", variable.Value);
                        message = message?.Replace($"{{{variable.Key}}}", variable.Value);
                    }
                }
            }

            var request = new NotificationRequest
            {
                UserId = userId,
                Title = title ?? template.Title,
                Message = message,
                Type = template.Type,
                Category = template.Category,
                Channels = template.Channels != null ? JsonSerializer.Deserialize<List<string>>(template.Channels) : new List<string> { "in-app" }
            };

            return await SendNotificationAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification from template: {TemplateName}", templateName);
            throw;
        }
    }

    public async Task<NotificationPreferencesResponse> GetPreferencesAsync(string userId)
    {
        try
        {
            var preferences = await _context.NotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (preferences == null)
            {
                // Create default preferences
                preferences = new NotificationPreferences
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    EmailEnabled = true,
                    SmsEnabled = false,
                    PushEnabled = true,
                    InAppEnabled = true,
                    EnabledCategories = JsonSerializer.Serialize(new List<string> { "security", "system", "user", "file" }),
                    UrgentNotifications = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.NotificationPreferences.Add(preferences);
                await _context.SaveChangesAsync();
            }

            return new NotificationPreferencesResponse
            {
                Id = preferences.Id.ToString(),
                UserId = preferences.UserId,
                EmailEnabled = preferences.EmailEnabled,
                SmsEnabled = preferences.SmsEnabled,
                PushEnabled = preferences.PushEnabled,
                InAppEnabled = preferences.InAppEnabled,
                EnabledCategories = preferences.EnabledCategories != null ? JsonSerializer.Deserialize<List<string>>(preferences.EnabledCategories) ?? new List<string>() : new List<string>(),
                UrgentNotifications = preferences.UrgentNotifications,
                EmailAddress = preferences.EmailAddress,
                PhoneNumber = preferences.PhoneNumber,
                CreatedAt = preferences.CreatedAt,
                UpdatedAt = preferences.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification preferences: {UserId}", userId);
            throw;
        }
    }

    public async Task<NotificationPreferencesResponse> UpdatePreferencesAsync(NotificationPreferencesRequest request)
    {
        try
        {
            var preferences = await _context.NotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == request.UserId);

            if (preferences == null)
            {
                preferences = new NotificationPreferences
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.NotificationPreferences.Add(preferences);
            }

            preferences.EmailEnabled = request.EmailEnabled;
            preferences.SmsEnabled = request.SmsEnabled;
            preferences.PushEnabled = request.PushEnabled;
            preferences.InAppEnabled = request.InAppEnabled;
            preferences.EnabledCategories = request.EnabledCategories != null ? JsonSerializer.Serialize(request.EnabledCategories) : JsonSerializer.Serialize(new List<string>());
            preferences.UrgentNotifications = request.UrgentNotifications;
            preferences.EmailAddress = request.EmailAddress;
            preferences.PhoneNumber = request.PhoneNumber;
            preferences.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification preferences updated: {UserId}", request.UserId);

            return new NotificationPreferencesResponse
            {
                Id = preferences.Id.ToString(),
                UserId = preferences.UserId,
                EmailEnabled = preferences.EmailEnabled,
                SmsEnabled = preferences.SmsEnabled,
                PushEnabled = preferences.PushEnabled,
                InAppEnabled = preferences.InAppEnabled,
                EnabledCategories = preferences.EnabledCategories != null ? JsonSerializer.Deserialize<List<string>>(preferences.EnabledCategories) ?? new List<string>() : new List<string>(),
                UrgentNotifications = preferences.UrgentNotifications,
                EmailAddress = preferences.EmailAddress,
                PhoneNumber = preferences.PhoneNumber,
                CreatedAt = preferences.CreatedAt,
                UpdatedAt = preferences.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification preferences: {UserId}", request.UserId);
            throw;
        }
    }

    public async Task SendRealTimeNotificationAsync(string userId, NotificationResponse notification)
    {
        try
        {
            // This would typically use SignalR or WebSocket to send real-time notifications
            // For now, we'll just log the notification
            _logger.LogInformation("Real-time notification sent to user {UserId}: {Title}", userId, notification.Title);
            
            // TODO: Implement actual real-time notification delivery
            // await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending real-time notification to user: {UserId}", userId);
        }
    }
} 