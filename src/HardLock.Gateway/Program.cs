using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Configuration;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HARDLOCK API Gateway", Version = "v1" });
    
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

// Configure YARP Reverse Proxy
var reverseProxy = builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Configure routes
var routes = new[]
{
    new RouteConfig
    {
        RouteId = "identity",
        ClusterId = "identity-cluster",
        Match = new RouteMatch
        {
            Path = "/api/auth/{**catch-all}"
        }
    },
    new RouteConfig
    {
        RouteId = "encryption",
        ClusterId = "encryption-cluster",
        Match = new RouteMatch
        {
            Path = "/api/encryption/{**catch-all}"
        }
    },
    new RouteConfig
    {
        RouteId = "storage",
        ClusterId = "storage-cluster",
        Match = new RouteMatch
        {
            Path = "/api/storage/{**catch-all}"
        }
    },
    new RouteConfig
    {
        RouteId = "access-control",
        ClusterId = "access-control-cluster",
        Match = new RouteMatch
        {
            Path = "/api/access/{**catch-all}"
        }
    },
    new RouteConfig
    {
        RouteId = "audit",
        ClusterId = "audit-cluster",
        Match = new RouteMatch
        {
            Path = "/api/audit/{**catch-all}"
        }
    },
    new RouteConfig
    {
        RouteId = "notification",
        ClusterId = "notification-cluster",
        Match = new RouteMatch
        {
            Path = "/api/notification/{**catch-all}"
        }
    }
};

var clusters = new[]
{
    new ClusterConfig
    {
        ClusterId = "identity-cluster",
        Destinations = new Dictionary<string, DestinationConfig>
        {
            { "identity-destination", new DestinationConfig { Address = "http://identity-service:8080" } }
        }
    },
    new ClusterConfig
    {
        ClusterId = "encryption-cluster",
        Destinations = new Dictionary<string, DestinationConfig>
        {
            { "encryption-destination", new DestinationConfig { Address = "http://encryption-service:8080" } }
        }
    },
    new ClusterConfig
    {
        ClusterId = "storage-cluster",
        Destinations = new Dictionary<string, DestinationConfig>
        {
            { "storage-destination", new DestinationConfig { Address = "http://storage-service:8080" } }
        }
    },
    new ClusterConfig
    {
        ClusterId = "access-control-cluster",
        Destinations = new Dictionary<string, DestinationConfig>
        {
            { "access-control-destination", new DestinationConfig { Address = "http://access-control-service:8080" } }
        }
    },
    new ClusterConfig
    {
        ClusterId = "audit-cluster",
        Destinations = new Dictionary<string, DestinationConfig>
        {
            { "audit-destination", new DestinationConfig { Address = "http://audit-service:8080" } }
        }
    },
    new ClusterConfig
    {
        ClusterId = "notification-cluster",
        Destinations = new Dictionary<string, DestinationConfig>
        {
            { "notification-destination", new DestinationConfig { Address = "http://notification-service:8080" } }
        }
    }
};

reverseProxy.LoadFromMemory(routes, clusters);

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

// Map reverse proxy
app.MapReverseProxy();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck");

app.Run(); 