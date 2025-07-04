using HardLock.Audit.Models;

namespace HardLock.Audit.Services;

public interface IAuditService
{
    // Audit Logging
    Task<AuditLogResponse> LogActivityAsync(AuditLogRequest request);
    Task<AuditSearchResponse> SearchAuditLogsAsync(AuditSearchRequest request);
    Task<AuditLogResponse?> GetAuditLogAsync(string logId);
    
    // Security Alerts
    Task<SecurityAlertResponse> CreateSecurityAlertAsync(SecurityAlertRequest request);
    Task<bool> ResolveSecurityAlertAsync(string alertId);
    Task<List<SecurityAlertResponse>> GetActiveAlertsAsync(string? severity = null);
    Task<SecurityAlertResponse?> GetSecurityAlertAsync(string alertId);
    
    // Reports
    Task<AuditReportResponse> GenerateAuditReportAsync(AuditReportRequest request);
    Task<string> ExportAuditLogsAsync(AuditSearchRequest request, string format = "json");
    
    // Analytics
    Task<Dictionary<string, int>> GetActionCountsAsync(DateTime fromDate, DateTime toDate, string? userId = null);
    Task<Dictionary<string, int>> GetSeverityCountsAsync(DateTime fromDate, DateTime toDate);
    Task<List<string>> GetTopUsersAsync(DateTime fromDate, DateTime toDate, int limit = 10);
    Task<List<string>> GetTopActionsAsync(DateTime fromDate, DateTime toDate, int limit = 10);
} 