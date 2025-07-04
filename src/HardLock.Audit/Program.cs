using Microsoft.EntityFrameworkCore;
using HardLock.Audit.Data;
using HardLock.Audit.Services;
using HardLock.Audit.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(builder.Configuration["Elasticsearch:Url"] ?? "http://localhost:9200"))
    {
        AutoRegisterTemplate = true,
        AutoRegisterTemplateVersion = Serilog.Sinks.Elasticsearch.AutoRegisterTemplateVersion.ESv7
    })
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddDbContext<AuditDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<IAuditService, AuditService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "HARDLOCK Audit Service", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Map endpoints
app.MapPost("/api/audit/log", async (AuditLogRequest request, IAuditService auditService) =>
{
    try
    {
        var result = await auditService.LogActivityAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error logging activity");
        return Results.BadRequest(new { Error = "Failed to log activity", Details = ex.Message });
    }
})
.WithName("LogActivity");

app.MapGet("/api/audit/search", async (IAuditService auditService, 
    string? userId = null, string? action = null, string? resourceType = null, 
    string? resourceId = null, string? severity = null, string? service = null,
    DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 20) =>
{
    try
    {
        var request = new AuditSearchRequest
        {
            UserId = userId,
            Action = action,
            ResourceType = resourceType,
            ResourceId = resourceId,
            Severity = severity,
            Service = service,
            FromDate = fromDate,
            ToDate = toDate,
            Page = page,
            PageSize = pageSize
        };

        var result = await auditService.SearchAuditLogsAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error searching audit logs");
        return Results.BadRequest(new { Error = "Failed to search audit logs", Details = ex.Message });
    }
})
.WithName("SearchAuditLogs");

app.MapGet("/api/audit/{logId}", async (string logId, IAuditService auditService) =>
{
    try
    {
        var result = await auditService.GetAuditLogAsync(logId);
        if (result == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error getting audit log");
        return Results.BadRequest(new { Error = "Failed to get audit log", Details = ex.Message });
    }
})
.WithName("GetAuditLog");

app.MapPost("/api/audit/alerts", async (SecurityAlertRequest request, IAuditService auditService) =>
{
    try
    {
        var result = await auditService.CreateSecurityAlertAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error creating security alert");
        return Results.BadRequest(new { Error = "Failed to create security alert", Details = ex.Message });
    }
})
.WithName("CreateSecurityAlert");

app.MapPut("/api/audit/alerts/{alertId}/resolve", async (string alertId, IAuditService auditService) =>
{
    try
    {
        var result = await auditService.ResolveSecurityAlertAsync(alertId);
        return Results.Ok(new { Resolved = result });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error resolving security alert");
        return Results.BadRequest(new { Error = "Failed to resolve security alert", Details = ex.Message });
    }
})
.WithName("ResolveSecurityAlert");

app.MapGet("/api/audit/alerts", async (IAuditService auditService, string? severity = null) =>
{
    try
    {
        var result = await auditService.GetActiveAlertsAsync(severity);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error getting active alerts");
        return Results.BadRequest(new { Error = "Failed to get active alerts", Details = ex.Message });
    }
})
.WithName("GetActiveAlerts");

app.MapGet("/api/audit/alerts/{alertId}", async (string alertId, IAuditService auditService) =>
{
    try
    {
        var result = await auditService.GetSecurityAlertAsync(alertId);
        if (result == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error getting security alert");
        return Results.BadRequest(new { Error = "Failed to get security alert", Details = ex.Message });
    }
})
.WithName("GetSecurityAlert");

app.MapPost("/api/audit/reports", async (AuditReportRequest request, IAuditService auditService) =>
{
    try
    {
        var result = await auditService.GenerateAuditReportAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error generating audit report");
        return Results.BadRequest(new { Error = "Failed to generate audit report", Details = ex.Message });
    }
})
.WithName("GenerateAuditReport");

app.MapGet("/api/audit/export", async (IAuditService auditService, 
    string? userId = null, string? action = null, string? resourceType = null, 
    string? resourceId = null, string? severity = null, string? service = null,
    DateTime? fromDate = null, DateTime? toDate = null, string format = "json") =>
{
    try
    {
        var request = new AuditSearchRequest
        {
            UserId = userId,
            Action = action,
            ResourceType = resourceType,
            ResourceId = resourceId,
            Severity = severity,
            Service = service,
            FromDate = fromDate,
            ToDate = toDate,
            Page = 1,
            PageSize = int.MaxValue
        };

        var result = await auditService.ExportAuditLogsAsync(request, format);
        return Results.Ok(new { Data = result, Format = format });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error exporting audit logs");
        return Results.BadRequest(new { Error = "Failed to export audit logs", Details = ex.Message });
    }
})
.WithName("ExportAuditLogs");

app.Run(); 