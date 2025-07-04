using HardLock.Notification.Models;

namespace HardLock.Notification.Services;

public interface INotificationService
{
    // Notification Management
    Task<NotificationResponse> SendNotificationAsync(NotificationRequest request);
    Task<BulkNotificationResponse> SendBulkNotificationAsync(BulkNotificationRequest request);
    Task<bool> MarkAsReadAsync(string notificationId, string userId);
    Task<bool> MarkAllAsReadAsync(string userId);
    Task<bool> DeleteNotificationAsync(string notificationId, string userId);
    
    // Notification Retrieval
    Task<NotificationSearchResponse> GetNotificationsAsync(NotificationSearchRequest request);
    Task<NotificationResponse?> GetNotificationAsync(string notificationId, string userId);
    Task<int> GetUnreadCountAsync(string userId);
    
    // Templates
    Task<NotificationTemplateResponse> CreateTemplateAsync(NotificationTemplateRequest request);
    Task<NotificationTemplateResponse> UpdateTemplateAsync(string templateId, NotificationTemplateRequest request);
    Task<bool> DeleteTemplateAsync(string templateId);
    Task<List<NotificationTemplateResponse>> GetTemplatesAsync();
    Task<NotificationTemplateResponse?> GetTemplateAsync(string templateId);
    Task<NotificationResponse> SendFromTemplateAsync(string templateName, string userId, Dictionary<string, string>? variables = null);
    
    // Preferences
    Task<NotificationPreferencesResponse> GetPreferencesAsync(string userId);
    Task<NotificationPreferencesResponse> UpdatePreferencesAsync(NotificationPreferencesRequest request);
    
    // Real-time
    Task SendRealTimeNotificationAsync(string userId, NotificationResponse notification);
} 