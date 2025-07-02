using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using HardLock.Identity.Data;
using HardLock.Identity.Services;
using HardLock.Identity.Validators;
using HardLock.Security.Encryption;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(builder.Configuration["Elasticsearch:Url"] ?? "http://localhost:9200"))
    {
        AutoRegisterTemplate = true,
        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7
    })
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Redis caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

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

// Add Dapper services
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IKeyDerivationService, KeyDerivationService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Register validators
builder.Services.AddScoped<UserRegistrationValidator>();
builder.Services.AddScoped<UserLoginValidator>();

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
    c.SwaggerDoc("v1", new() { Title = "HARDLOCK Identity Service", Version = "v1" });
    
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
app.MapPost("/api/auth/register", async (UserRegistrationRequest request, IUserService userService, UserRegistrationValidator validator) =>
{
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors);
    }

    try
    {
        var user = await userService.RegisterUserAsync(request);
        return Results.Ok(new { Message = "User registered successfully", UserId = user.Id });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during user registration");
        return Results.BadRequest(new { Error = "Registration failed", Details = ex.Message });
    }
})
.WithName("RegisterUser")
.WithOpenApi();

app.MapPost("/api/auth/login", async (UserLoginRequest request, IAuthService authService, UserLoginValidator validator) =>
{
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors);
    }

    try
    {
        var result = await authService.LoginAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during user login");
        return Results.Unauthorized();
    }
})
.WithName("LoginUser")
.WithOpenApi();

app.MapPost("/api/auth/refresh", async (RefreshTokenRequest request, IAuthService authService) =>
{
    try
    {
        var result = await authService.RefreshTokenAsync(request.RefreshToken);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during token refresh");
        return Results.Unauthorized();
    }
})
.WithName("RefreshToken")
.WithOpenApi();

app.MapGet("/api/auth/me", async (IUserService userService, HttpContext context) =>
{
    var userId = context.User.FindFirst("sub")?.Value;
    if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
    {
        return Results.Unauthorized();
    }

    try
    {
        var user = await userService.GetUserByIdAsync(id);
        return Results.Ok(user);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error getting user profile");
        return Results.NotFound();
    }
})
.WithName("GetUserProfile")
.WithOpenApi()
.RequireAuthorization();

app.MapPut("/api/auth/me", async (UserUpdateRequest request, IUserService userService, HttpContext context) =>
{
    var userId = context.User.FindFirst("sub")?.Value;
    if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
    {
        return Results.Unauthorized();
    }

    try
    {
        var user = await userService.UpdateUserAsync(id, request);
        return Results.Ok(user);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error updating user profile");
        return Results.BadRequest(new { Error = "Update failed", Details = ex.Message });
    }
})
.WithName("UpdateUserProfile")
.WithOpenApi()
.RequireAuthorization();

app.MapPost("/api/auth/logout", async (IUserService userService, HttpContext context) =>
{
    var userId = context.User.FindFirst("sub")?.Value;
    if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var id))
    {
        await userService.LogoutAsync(id);
    }
    
    return Results.Ok(new { Message = "Logged out successfully" });
})
.WithName("LogoutUser")
.WithOpenApi()
.RequireAuthorization();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithOpenApi();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run(); 