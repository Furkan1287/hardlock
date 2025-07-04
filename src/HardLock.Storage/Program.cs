using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Logging;
using HardLock.Storage.Data;
using HardLock.Storage.Services;
using HardLock.Storage.Models;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<StorageDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured")))
        };
    });

builder.Services.AddAuthorization();

// Register services
builder.Services.AddScoped<IStorageService, StorageService>();

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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HARDLOCK Storage Service", Version = "v1" });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
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
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapPost("/api/storage/upload", async (FileStorageRequest request, IStorageService storageService) =>
{
    try
    {
        var result = await storageService.StoreFileAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Failed to store file", Details = ex.Message });
    }
})
.WithName("StoreFile");

app.MapGet("/api/storage/files/{fileId}", async (string fileId, string? userId, IStorageService storageService) =>
{
    try
    {
        var request = new FileRetrievalRequest { FileId = fileId, UserId = userId };
        var result = await storageService.RetrieveFileAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Failed to retrieve file", Details = ex.Message });
    }
})
.WithName("RetrieveFile");

app.MapDelete("/api/storage/files/{fileId}", async (string fileId, string? userId, IStorageService storageService) =>
{
    try
    {
        var request = new FileDeleteRequest { FileId = fileId, UserId = userId };
        var result = await storageService.DeleteFileAsync(request);
        return result ? Results.Ok() : Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Failed to delete file", Details = ex.Message });
    }
})
.WithName("DeleteFile");

app.MapGet("/api/storage/files", async (IStorageService storageService, string? userId, string? fileType, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 20) =>
{
    try
    {
        var request = new FileListRequest 
        { 
            UserId = userId, 
            FileType = fileType, 
            FromDate = fromDate, 
            ToDate = toDate, 
            Page = page, 
            PageSize = pageSize 
        };
        var result = await storageService.ListFilesAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Failed to list files", Details = ex.Message });
    }
})
.WithName("ListFiles");

app.MapGet("/api/storage/usage", async (IStorageService storageService, HttpContext context) =>
{
    try
    {
        var userId = context.User.FindFirst("sub")?.Value;
        var usage = await storageService.GetStorageUsageAsync(userId);
        return Results.Ok(new { UsageBytes = usage, UsageMB = usage / (1024.0 * 1024.0) });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Usage calculation failed", Details = ex.Message });
    }
})
.WithName("GetStorageUsage");

app.MapPost("/api/storage/files/{fileId}/move-to-cold", async (string fileId, IStorageService storageService, HttpContext context) =>
{
    try
    {
        var userId = context.User.FindFirst("sub")?.Value;
        var result = await storageService.MoveToColdStorageAsync(fileId, userId);
        
        if (result)
        {
            return Results.Ok(new { Message = "File moved to cold storage successfully" });
        }
        else
        {
            return Results.NotFound();
        }
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Move to cold storage failed", Details = ex.Message });
    }
})
.WithName("MoveToColdStorage");

app.MapPost("/api/storage/files/{fileId}/restore-from-cold", async (string fileId, IStorageService storageService, HttpContext context) =>
{
    try
    {
        var userId = context.User.FindFirst("sub")?.Value;
        var result = await storageService.RestoreFromColdStorageAsync(fileId, userId);
        
        if (result)
        {
            return Results.Ok(new { Message = "File restored from cold storage successfully" });
        }
        else
        {
            return Results.NotFound();
        }
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Restore from cold storage failed", Details = ex.Message });
    }
})
.WithName("RestoreFromColdStorage");

app.Run(); 