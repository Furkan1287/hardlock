using HardLock.Audit.Models;
using HardLock.Audit.Data;
using HardLock.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HardLock.Audit.Services;

public class AuditService : IAuditService
{
    private readonly AuditDbContext _context;
    private readonly Microsoft.Extensions.Logging.ILogger<AuditService> _logger;

    public AuditService(AuditDbContext context, Microsoft.Extensions.Logging.ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AuditLogResponse> LogActivityAsync(AuditLogRequest request)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Action = request.Action,
                ResourceType = request.ResourceType,
                ResourceId = request.ResourceId,
                Description = request.Description,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                Location = request.Location,
                Metadata = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null,
                Severity = request.Severity ?? "info",
                Service = request.Service,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Audit log created: {UserId} - {Action} - {ResourceType}", 
                request.UserId, request.Action, request.ResourceType);

            return new AuditLogResponse
            {
                Id = auditLog.Id.ToString(),
                UserId = auditLog.UserId,
                Action = auditLog.Action,
                ResourceType = auditLog.ResourceType,
                ResourceId = auditLog.ResourceId,
                Description = auditLog.Description,
                IpAddress = auditLog.IpAddress,
                UserAgent = auditLog.UserAgent,
                Location = auditLog.Location,
                Metadata = auditLog.Metadata != null ? JsonSerializer.Deserialize<Dictionary<string, object>>(auditLog.Metadata) : null,
                Severity = auditLog.Severity,
                Service = auditLog.Service,
                Timestamp = auditLog.Timestamp
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging activity");
            throw;
        }
    }

    public async Task<AuditSearchResponse> SearchAuditLogsAsync(AuditSearchRequest request)
    {
        try
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(request.UserId))
            {
                query = query.Where(a => a.UserId == request.UserId);
            }

            if (!string.IsNullOrEmpty(request.Action))
            {
                query = query.Where(a => a.Action == request.Action);
            }

            if (!string.IsNullOrEmpty(request.ResourceType))
            {
                query = query.Where(a => a.ResourceType == request.ResourceType);
            }

            if (!string.IsNullOrEmpty(request.ResourceId))
            {
                query = query.Where(a => a.ResourceId == request.ResourceId);
            }

            if (!string.IsNullOrEmpty(request.Severity))
            {
                query = query.Where(a => a.Severity == request.Severity);
            }

            if (!string.IsNullOrEmpty(request.Service))
            {
                query = query.Where(a => a.Service == request.Service);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= request.ToDate.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new AuditLogResponse
                {
                    Id = a.Id.ToString(),
                    UserId = a.UserId,
                    Action = a.Action,
                    ResourceType = a.ResourceType,
                    ResourceId = a.ResourceId,
                    Description = a.Description,
                    IpAddress = a.IpAddress,
                    UserAgent = a.UserAgent,
                    Location = a.Location,
                    Metadata = null, // Will be deserialized after query
                    Severity = a.Severity,
                    Service = a.Service,
                    Timestamp = a.Timestamp
                })
                .ToListAsync();

            // Deserialize metadata after query
            foreach (var log in logs)
            {
                var originalLog = await _context.AuditLogs.FindAsync(Guid.Parse(log.Id));
                if (originalLog?.Metadata != null)
                {
                    log.Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(originalLog.Metadata);
                }
            }

            return new AuditSearchResponse
            {
                Logs = logs,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching audit logs");
            throw;
        }
    }

    public async Task<AuditLogResponse?> GetAuditLogAsync(string logId)
    {
        try
        {
            if (!Guid.TryParse(logId, out var id))
            {
                return null;
            }

            var auditLog = await _context.AuditLogs.FindAsync(id);
            if (auditLog == null)
            {
                return null;
            }

            return new AuditLogResponse
            {
                Id = auditLog.Id.ToString(),
                UserId = auditLog.UserId,
                Action = auditLog.Action,
                ResourceType = auditLog.ResourceType,
                ResourceId = auditLog.ResourceId,
                Description = auditLog.Description,
                IpAddress = auditLog.IpAddress,
                UserAgent = auditLog.UserAgent,
                Location = auditLog.Location,
                Metadata = auditLog.Metadata != null ? JsonSerializer.Deserialize<Dictionary<string, object>>(auditLog.Metadata) : null,
                Severity = auditLog.Severity,
                Service = auditLog.Service,
                Timestamp = auditLog.Timestamp
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit log: {LogId}", logId);
            return null;
        }
    }

    public async Task<SecurityAlertResponse> CreateSecurityAlertAsync(SecurityAlertRequest request)
    {
        try
        {
            var alert = new SecurityAlert
            {
                Id = Guid.NewGuid(),
                AlertType = request.AlertType,
                Title = request.Title,
                Description = request.Description,
                Severity = request.Severity,
                UserId = request.UserId,
                ResourceId = request.ResourceId,
                IpAddress = request.IpAddress,
                Context = request.Context != null ? JsonSerializer.Serialize(request.Context) : null,
                CreatedAt = DateTime.UtcNow,
                IsResolved = false
            };

            _context.SecurityAlerts.Add(alert);
            await _context.SaveChangesAsync();

            _logger.LogWarning("Security alert created: {AlertType} - {Title} - {Severity}", 
                request.AlertType, request.Title, request.Severity);

            return new SecurityAlertResponse
            {
                Id = alert.Id.ToString(),
                AlertType = alert.AlertType,
                Title = alert.Title,
                Description = alert.Description,
                Severity = alert.Severity,
                UserId = alert.UserId,
                ResourceId = alert.ResourceId,
                IpAddress = alert.IpAddress,
                Context = alert.Context != null ? JsonSerializer.Deserialize<Dictionary<string, object>>(alert.Context) : null,
                CreatedAt = alert.CreatedAt,
                IsResolved = alert.IsResolved,
                ResolvedAt = alert.ResolvedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating security alert");
            throw;
        }
    }

    public async Task<bool> ResolveSecurityAlertAsync(string alertId)
    {
        try
        {
            if (!Guid.TryParse(alertId, out var id))
            {
                return false;
            }

            var alert = await _context.SecurityAlerts.FindAsync(id);
            if (alert == null)
            {
                return false;
            }

            alert.IsResolved = true;
            alert.ResolvedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Security alert resolved: {AlertId}", alertId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving security alert: {AlertId}", alertId);
            return false;
        }
    }

    public async Task<List<SecurityAlertResponse>> GetActiveAlertsAsync(string? severity = null)
    {
        try
        {
            var query = _context.SecurityAlerts.Where(a => !a.IsResolved);

            if (!string.IsNullOrEmpty(severity))
            {
                query = query.Where(a => a.Severity == severity);
            }

            var alerts = await query
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new SecurityAlertResponse
                {
                    Id = a.Id.ToString(),
                    AlertType = a.AlertType,
                    Title = a.Title,
                    Description = a.Description,
                    Severity = a.Severity,
                    UserId = a.UserId,
                    ResourceId = a.ResourceId,
                    IpAddress = a.IpAddress,
                    Context = null, // Will be deserialized after query
                    CreatedAt = a.CreatedAt,
                    IsResolved = a.IsResolved,
                    ResolvedAt = a.ResolvedAt
                })
                .ToListAsync();

            // Deserialize context after query
            foreach (var alert in alerts)
            {
                var originalAlert = await _context.SecurityAlerts.FindAsync(Guid.Parse(alert.Id));
                if (originalAlert?.Context != null)
                {
                    alert.Context = JsonSerializer.Deserialize<Dictionary<string, object>>(originalAlert.Context);
                }
            }

            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active alerts");
            throw;
        }
    }

    public async Task<SecurityAlertResponse?> GetSecurityAlertAsync(string alertId)
    {
        try
        {
            if (!Guid.TryParse(alertId, out var id))
            {
                return null;
            }

            var alert = await _context.SecurityAlerts.FindAsync(id);
            if (alert == null)
            {
                return null;
            }

            return new SecurityAlertResponse
            {
                Id = alert.Id.ToString(),
                AlertType = alert.AlertType,
                Title = alert.Title,
                Description = alert.Description,
                Severity = alert.Severity,
                UserId = alert.UserId,
                ResourceId = alert.ResourceId,
                IpAddress = alert.IpAddress,
                Context = alert.Context != null ? JsonSerializer.Deserialize<Dictionary<string, object>>(alert.Context) : null,
                CreatedAt = alert.CreatedAt,
                IsResolved = alert.IsResolved,
                ResolvedAt = alert.ResolvedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security alert: {AlertId}", alertId);
            return null;
        }
    }

    public async Task<AuditReportResponse> GenerateAuditReportAsync(AuditReportRequest request)
    {
        try
        {
            var query = _context.AuditLogs
                .Where(a => a.Timestamp >= request.FromDate && a.Timestamp <= request.ToDate);

            if (!string.IsNullOrEmpty(request.UserId))
            {
                query = query.Where(a => a.UserId == request.UserId);
            }

            if (!string.IsNullOrEmpty(request.Service))
            {
                query = query.Where(a => a.Service == request.Service);
            }

            var logs = await query.ToListAsync();
            var alerts = await _context.SecurityAlerts
                .Where(a => a.CreatedAt >= request.FromDate && a.CreatedAt <= request.ToDate)
                .ToListAsync();

            var actionCounts = logs.GroupBy(a => a.Action)
                .ToDictionary(g => g.Key, g => g.Count());

            var severityCounts = logs.GroupBy(a => a.Severity)
                .ToDictionary(g => g.Key, g => g.Count());

            var serviceCounts = logs.GroupBy(a => a.Service ?? "unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            var securityAlerts = alerts.Select(a => new SecurityAlertResponse
            {
                Id = a.Id.ToString(),
                AlertType = a.AlertType,
                Title = a.Title,
                Description = a.Description,
                Severity = a.Severity,
                UserId = a.UserId,
                ResourceId = a.ResourceId,
                IpAddress = a.IpAddress,
                Context = null, // Will be deserialized after query
                CreatedAt = a.CreatedAt,
                IsResolved = a.IsResolved,
                ResolvedAt = a.ResolvedAt
            }).ToList();

            // Deserialize context after query
            foreach (var alert in securityAlerts)
            {
                var originalAlert = await _context.SecurityAlerts.FindAsync(Guid.Parse(alert.Id));
                if (originalAlert?.Context != null)
                {
                    alert.Context = JsonSerializer.Deserialize<Dictionary<string, object>>(originalAlert.Context);
                }
            }

            var uniqueUsers = logs.Select(a => a.UserId).Distinct().Count();

            var reportData = JsonSerializer.Serialize(new
            {
                Logs = logs,
                Alerts = alerts,
                Summary = new
                {
                    TotalLogs = logs.Count,
                    UniqueUsers = uniqueUsers,
                    ActionCounts = actionCounts,
                    SeverityCounts = severityCounts,
                    ServiceCounts = serviceCounts
                }
            });

            return new AuditReportResponse
            {
                ReportId = Guid.NewGuid().ToString(),
                GeneratedAt = DateTime.UtcNow,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                ActionCounts = actionCounts,
                SeverityCounts = severityCounts,
                ServiceCounts = serviceCounts,
                SecurityAlerts = securityAlerts,
                TotalLogs = logs.Count,
                UniqueUsers = uniqueUsers,
                ReportData = reportData
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating audit report");
            throw;
        }
    }

    public async Task<string> ExportAuditLogsAsync(AuditSearchRequest request, string format = "json")
    {
        try
        {
            var searchResult = await SearchAuditLogsAsync(request);

            if (format.ToLower() == "csv")
            {
                var csv = "Id,UserId,Action,ResourceType,ResourceId,Description,IpAddress,UserAgent,Location,Severity,Service,Timestamp\n";
                foreach (var log in searchResult.Logs)
                {
                    csv += $"{log.Id},{log.UserId},{log.Action},{log.ResourceType},{log.ResourceId ?? ""},{log.Description ?? ""},{log.IpAddress ?? ""},{log.UserAgent ?? ""},{log.Location ?? ""},{log.Severity},{log.Service ?? ""},{log.Timestamp:yyyy-MM-dd HH:mm:ss}\n";
                }
                return csv;
            }
            else
            {
                return JsonSerializer.Serialize(searchResult.Logs, new JsonSerializerOptions { WriteIndented = true });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting audit logs");
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetActionCountsAsync(DateTime fromDate, DateTime toDate, string? userId = null)
    {
        try
        {
            var query = _context.AuditLogs
                .Where(a => a.Timestamp >= fromDate && a.Timestamp <= toDate);

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(a => a.UserId == userId);
            }

            var counts = await query
                .GroupBy(a => a.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Action, x => x.Count);

            return counts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting action counts");
            return new Dictionary<string, int>();
        }
    }

    public async Task<Dictionary<string, int>> GetSeverityCountsAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            var counts = await _context.AuditLogs
                .Where(a => a.Timestamp >= fromDate && a.Timestamp <= toDate)
                .GroupBy(a => a.Severity)
                .Select(g => new { Severity = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Severity, x => x.Count);

            return counts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting severity counts");
            return new Dictionary<string, int>();
        }
    }

    public async Task<List<string>> GetTopUsersAsync(DateTime fromDate, DateTime toDate, int limit = 10)
    {
        try
        {
            var users = await _context.AuditLogs
                .Where(a => a.Timestamp >= fromDate && a.Timestamp <= toDate)
                .GroupBy(a => a.UserId)
                .OrderByDescending(g => g.Count())
                .Take(limit)
                .Select(g => g.Key)
                .ToListAsync();

            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top users");
            return new List<string>();
        }
    }

    public async Task<List<string>> GetTopActionsAsync(DateTime fromDate, DateTime toDate, int limit = 10)
    {
        try
        {
            var actions = await _context.AuditLogs
                .Where(a => a.Timestamp >= fromDate && a.Timestamp <= toDate)
                .GroupBy(a => a.Action)
                .OrderByDescending(g => g.Count())
                .Take(limit)
                .Select(g => g.Key)
                .ToListAsync();

            return actions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top actions");
            return new List<string>();
        }
    }
} 