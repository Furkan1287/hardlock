using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using HardLock.Encryption.Services;
using HardLock.Encryption.Validators;
using HardLock.Security.Encryption;
using HardLock.Encryption.Models;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

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
builder.Services.AddScoped<IFileEncryptionService, FileEncryptionService>();
builder.Services.AddScoped<IKeyDerivationService, KeyDerivationService>();
builder.Services.AddScoped<ITimelockService, TimelockService>();
builder.Services.AddScoped<IEthereumBlockService, EthereumBlockService>();
builder.Services.AddScoped<IDarknetBackupService, DarknetBackupService>();
builder.Services.AddScoped<IGeoFencingService, GeoFencingService>();
builder.Services.AddScoped<IFileHashingService, FileHashingService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

// Register validators
builder.Services.AddScoped<FileEncryptionRequestValidator>();

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
    c.SwaggerDoc("v1", new() { Title = "HARDLOCK Encryption Service", Version = "v1" });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new() { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
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
app.MapPost("/api/encrypt", async (FileEncryptionRequest request, IEncryptionService encryptionService, FileEncryptionRequestValidator validator) =>
{
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors);
    }

    try
    {
        var result = await encryptionService.EncryptFileAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during file encryption");
        return Results.BadRequest(new { Error = "Encryption failed", Details = ex.Message });
    }
})
.WithName("EncryptFile")
.RequireAuthorization();

app.MapPost("/api/decrypt", async (FileDecryptionRequest request, IEncryptionService encryptionService) =>
{
    try
    {
        var result = await encryptionService.DecryptFileAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during file decryption");
        return Results.BadRequest(new { Error = "Decryption failed", Details = ex.Message });
    }
})
.WithName("DecryptFile")
.RequireAuthorization();

app.MapPost("/api/encrypt/shard", async (ShardEncryptionRequest request, IEncryptionService encryptionService) =>
{
    try
    {
        var result = await encryptionService.ShardEncryptAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during sharded encryption");
        return Results.BadRequest(new { Error = "Sharded encryption failed", Details = ex.Message });
    }
})
.WithName("ShardEncryptFile")
.RequireAuthorization();

app.MapPost("/api/decrypt/shard", async (ShardDecryptionRequest request, IEncryptionService encryptionService) =>
{
    try
    {
        var result = await encryptionService.ShardDecryptAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during sharded decryption");
        return Results.BadRequest(new { Error = "Sharded decryption failed", Details = ex.Message });
    }
})
.WithName("ShardDecryptFile")
.RequireAuthorization();

app.MapPost("/api/validate-password", async (PasswordValidationRequest request, IEncryptionService encryptionService) =>
{
    try
    {
        var isValid = await encryptionService.ValidatePasswordAsync(request);
        return Results.Ok(new { IsValid = isValid });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during password validation");
        return Results.BadRequest(new { Error = "Password validation failed", Details = ex.Message });
    }
})
.WithName("ValidatePassword")
.RequireAuthorization();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck");

// Timelock Endpoints
app.MapPost("/api/timelock/encrypt", async (TimelockEncryptionRequest request, IEncryptionService encryptionService) =>
{
    try
    {
        var result = await encryptionService.EncryptWithTimelockAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during timelock encryption");
        return Results.BadRequest(new { Error = "Timelock encryption failed", Details = ex.Message });
    }
})
.WithName("TimelockEncrypt")
.RequireAuthorization();

app.MapPost("/api/timelock/decrypt", async (TimelockDecryptionRequest request, IEncryptionService encryptionService) =>
{
    try
    {
        var result = await encryptionService.DecryptWithTimelockAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during timelock decryption");
        return Results.BadRequest(new { Error = "Timelock decryption failed", Details = ex.Message });
    }
})
.WithName("TimelockDecrypt")
.RequireAuthorization();

// Darknet Backup Endpoints
app.MapPost("/api/darknet/backup", async (DarknetBackupRequest request, IDarknetBackupService darknetService) =>
{
    try
    {
        var options = new DarknetBackupOptions
        {
            ShardSize = request.ShardSize,
            EncryptionKey = request.EncryptionKey,
            ExpiresAt = request.ExpiresAt,
            ReplicationFactor = request.ReplicationFactor,
            EnableTorRouting = request.EnableTorRouting
        };

        var result = await darknetService.BackupToDarknetAsync(request.FileData, request.FileName, options);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during darknet backup");
        return Results.BadRequest(new { Error = "Darknet backup failed", Details = ex.Message });
    }
})
.WithName("DarknetBackup")
.RequireAuthorization();

app.MapPost("/api/darknet/restore", async (DarknetRestoreRequest request, IDarknetBackupService darknetService) =>
{
    try
    {
        var options = new DarknetBackupOptions
        {
            EncryptionKey = request.EncryptionKey
        };

        var result = await darknetService.RestoreFromDarknetAsync(request.ContentHash, request.FileName, options);
        if (result != null)
        {
            return Results.Ok(new { FileData = Convert.ToBase64String(result), FileName = request.FileName });
        }
        return Results.NotFound(new { Error = "File not found in darknet" });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during darknet restore");
        return Results.BadRequest(new { Error = "Darknet restore failed", Details = ex.Message });
    }
})
.WithName("DarknetRestore")
.RequireAuthorization();

app.MapGet("/api/darknet/status/{contentHash}", async (string contentHash, IDarknetBackupService darknetService) =>
{
    try
    {
        var status = await darknetService.GetBackupStatusAsync(contentHash);
        return Results.Ok(status);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error getting darknet backup status");
        return Results.BadRequest(new { Error = "Failed to get backup status", Details = ex.Message });
    }
})
.WithName("DarknetStatus")
.RequireAuthorization();

app.MapGet("/api/darknet/nodes", async (IDarknetBackupService darknetService) =>
{
    try
    {
        var nodes = await darknetService.GetAvailableNodesAsync();
        return Results.Ok(nodes);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error getting darknet nodes");
        return Results.BadRequest(new { Error = "Failed to get nodes", Details = ex.Message });
    }
})
.WithName("DarknetNodes")
.RequireAuthorization();

// Geo-Fencing Endpoints
app.MapPost("/api/geo-fencing/validate", async (GeoFencingValidationRequest request, IGeoFencingService geoFencingService) =>
{
    try
    {
        var userLocation = new HardLock.Security.Encryption.GeoLocation
        {
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IPAddress = request.IPAddress
        };

        var rules = new GeoFencingRules
        {
            IsEnabled = request.IsEnabled,
            AllowedCountries = request.AllowedCountries,
            AllowedCities = request.AllowedCities,
            AllowedLocations = request.AllowedLocations,
            AllowedPolygons = request.AllowedPolygons?.Select(polygon => 
                polygon.Select(loc => new HardLock.Security.Encryption.GeoLocation 
                { 
                    Latitude = loc.Latitude, 
                    Longitude = loc.Longitude 
                }).ToList()).ToList()
        };

        var isValid = await geoFencingService.ValidateAccessAsync(userLocation, rules);
        return Results.Ok(new { IsValid = isValid.IsAllowed });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during geo-fencing validation");
        return Results.BadRequest(new { Error = "Geo-fencing validation failed", Details = ex.Message });
    }
})
.WithName("GeoFencingValidate")
.RequireAuthorization();

// File Hashing Endpoints
app.MapPost("/api/hash/file", async (FileHashRequest request, IFileHashingService fileHashingService) =>
{
    try
    {
        FileHashResult result;
        
        if (!string.IsNullOrEmpty(request.FilePath))
        {
            if (request.IsLargeFile)
            {
                result = await fileHashingService.HashLargeFileAsync(request.FilePath, ParseHashAlgorithm(request.HashAlgorithm), request.BufferSize ?? 8192);
            }
            else
            {
                result = await fileHashingService.HashFileAsync(request.FilePath, ParseHashAlgorithm(request.HashAlgorithm));
            }
        }
        else if (request.FileData != null)
        {
            result = await fileHashingService.HashFileAsync(request.FileData, ParseHashAlgorithm(request.HashAlgorithm));
        }
        else
        {
            return Results.BadRequest(new { Error = "Either FilePath or FileData must be provided" });
        }

        var response = new FileHashResponse
        {
            Success = result.Success,
            Hash = result.Hash,
            Algorithm = result.Algorithm.ToString(),
            FilePath = result.FilePath,
            FileSize = result.FileSize,
            HashTime = result.HashTime,
            IsLargeFile = result.IsLargeFile,
            ErrorMessage = result.ErrorMessage
        };

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during file hashing");
        return Results.BadRequest(new { Error = "File hashing failed", Details = ex.Message });
    }
})
.WithName("HashFile")
.RequireAuthorization();

app.MapPost("/api/hash/verify", async (FileIntegrityRequest request, IFileHashingService fileHashingService) =>
{
    try
    {
        var integrityInfo = new FileIntegrityInfo
        {
            Hash = request.ExpectedHash,
            Algorithm = ParseHashAlgorithm(request.HashAlgorithm),
            FilePath = request.FilePath
        };

        var result = await fileHashingService.VerifyFileIntegrityAsync(request.FilePath, integrityInfo);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during file integrity verification");
        return Results.BadRequest(new { Error = "File integrity verification failed", Details = ex.Message });
    }
})
.WithName("VerifyFileIntegrity")
.RequireAuthorization();

app.MapPost("/api/hash/create-info", async (CreateIntegrityInfoRequest request, IFileHashingService fileHashingService) =>
{
    try
    {
        FileIntegrityInfo result;
        
        if (!string.IsNullOrEmpty(request.FilePath))
        {
            result = await fileHashingService.CreateIntegrityInfoAsync(request.FilePath, ParseHashAlgorithm(request.HashAlgorithm));
        }
        else if (request.FileData != null)
        {
            result = await fileHashingService.CreateIntegrityInfoAsync(request.FileData, ParseHashAlgorithm(request.HashAlgorithm));
        }
        else
        {
            return Results.BadRequest(new { Error = "Either FilePath or FileData must be provided" });
        }

        var response = new IntegrityInfoResponse
        {
            Hash = result.Hash,
            Algorithm = result.Algorithm.ToString(),
            FilePath = result.FilePath,
            FileSize = result.FileSize,
            CreatedAt = result.CreatedAt.GetValueOrDefault(DateTime.UtcNow),
            Metadata = result.Metadata
        };

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error creating integrity info");
        return Results.BadRequest(new { Error = "Creating integrity info failed", Details = ex.Message });
    }
})
.WithName("CreateIntegrityInfo")
.RequireAuthorization();

app.MapPost("/api/hash/verify-info", async (FileIntegrityRequest request, IFileHashingService fileHashingService) =>
{
    try
    {
        var integrityInfo = new FileIntegrityInfo
        {
            Hash = request.ExpectedHash,
            Algorithm = ParseHashAlgorithm(request.HashAlgorithm),
            FilePath = request.FilePath
        };

        var result = await fileHashingService.VerifyFileIntegrityAsync(request.FilePath, integrityInfo);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during integrity info verification");
        return Results.BadRequest(new { Error = "Integrity info verification failed", Details = ex.Message });
    }
})
.WithName("VerifyIntegrityInfo")
.RequireAuthorization();

// Helper method to parse hash algorithm
static HashAlgorithm ParseHashAlgorithm(string algorithm) => algorithm.ToUpperInvariant() switch
{
    "MD5" => HashAlgorithm.MD5,
    "SHA1" => HashAlgorithm.SHA1,
    "SHA256" => HashAlgorithm.SHA256,
    "SHA384" => HashAlgorithm.SHA384,
    "SHA512" => HashAlgorithm.SHA512,
    _ => HashAlgorithm.SHA256
};

app.Run(); 