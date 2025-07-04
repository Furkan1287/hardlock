using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Logging;
using HardLock.AccessControl.Data;
using HardLock.AccessControl.Services;
using HardLock.AccessControl.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<AccessControlDbContext>(options =>
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
builder.Services.AddScoped<IAccessControlService, AccessControlService>();

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
    c.SwaggerDoc("v1", new() { Title = "HARDLOCK Access Control Service", Version = "v1" });
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
app.MapPost("/api/access/permissions", async (PermissionRequest request, IAccessControlService accessControlService) =>
{
    try
    {
        var result = await accessControlService.GrantPermissionAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Permission grant failed", Details = ex.Message });
    }
})
.WithName("GrantPermission");

app.MapDelete("/api/access/permissions/{permissionId}", async (string permissionId, IAccessControlService accessControlService) =>
{
    try
    {
        var result = await accessControlService.RevokePermissionAsync(permissionId);
        if (result)
        {
            return Results.Ok(new { Message = "Permission revoked successfully" });
        }
        else
        {
            return Results.NotFound();
        }
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Permission revocation failed", Details = ex.Message });
    }
})
.WithName("RevokePermission");

app.MapGet("/api/access/permissions/user/{userId}", async (string userId, IAccessControlService accessControlService, string? resourceType = null) =>
{
    try
    {
        var result = await accessControlService.GetUserPermissionsAsync(userId, resourceType);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Failed to get user permissions", Details = ex.Message });
    }
})
.WithName("GetUserPermissions");

app.MapGet("/api/access/permissions/resource/{resourceId}", async (string resourceId, string resourceType, IAccessControlService accessControlService) =>
{
    try
    {
        var result = await accessControlService.GetResourcePermissionsAsync(resourceId, resourceType);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Failed to get resource permissions", Details = ex.Message });
    }
})
.WithName("GetResourcePermissions");

app.MapPost("/api/access/check", async (AccessCheckRequest request, IAccessControlService accessControlService) =>
{
    try
    {
        var result = await accessControlService.CheckAccessAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Access check failed", Details = ex.Message });
    }
})
.WithName("CheckAccess");

app.MapPost("/api/access/roles", async (RoleRequest request, IAccessControlService accessControlService) =>
{
    try
    {
        var result = await accessControlService.CreateRoleAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Role creation failed", Details = ex.Message });
    }
})
.WithName("CreateRole");

app.MapPut("/api/access/roles/{roleId}", async (string roleId, RoleRequest request, IAccessControlService accessControlService) =>
{
    try
    {
        var result = await accessControlService.UpdateRoleAsync(roleId, request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Role update failed", Details = ex.Message });
    }
})
.WithName("UpdateRole");

app.MapDelete("/api/access/roles/{roleId}", async (string roleId, IAccessControlService accessControlService) =>
{
    try
    {
        var result = await accessControlService.DeleteRoleAsync(roleId);
        if (result)
        {
            return Results.Ok(new { Message = "Role deleted successfully" });
        }
        else
        {
            return Results.NotFound();
        }
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Role deletion failed", Details = ex.Message });
    }
})
.WithName("DeleteRole");

app.MapGet("/api/access/roles", async (IAccessControlService accessControlService, string? organizationId = null) =>
{
    try
    {
        var result = await accessControlService.GetRolesAsync(organizationId);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Failed to get roles", Details = ex.Message });
    }
})
.WithName("GetRoles");

app.MapGet("/api/access/roles/{roleId}", async (string roleId, IAccessControlService accessControlService) =>
{
    try
    {
        var result = await accessControlService.GetRoleAsync(roleId);
        if (result == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Failed to get role", Details = ex.Message });
    }
})
.WithName("GetRole");

app.MapPost("/api/access/user-roles", async (UserRoleRequest request, IAccessControlService accessControlService) =>
{
    try
    {
        var result = await accessControlService.AssignRoleToUserAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Role assignment failed", Details = ex.Message });
    }
})
.WithName("AssignRoleToUser");

app.MapDelete("/api/access/user-roles/{userRoleId}", async (string userRoleId, IAccessControlService accessControlService) =>
{
    try
    {
        var result = await accessControlService.RemoveRoleFromUserAsync(userRoleId);
        if (result)
        {
            return Results.Ok(new { Message = "Role removed successfully" });
        }
        else
        {
            return Results.NotFound();
        }
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Role removal failed", Details = ex.Message });
    }
})
.WithName("RemoveRoleFromUser");

app.MapGet("/api/access/user-roles/user/{userId}", async (string userId, IAccessControlService accessControlService) =>
{
    try
    {
        var result = await accessControlService.GetUserRolesAsync(userId);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Failed to get user roles", Details = ex.Message });
    }
})
.WithName("GetUserRoles");

app.MapGet("/api/access/user-roles/role/{roleId}", async (string roleId, IAccessControlService accessControlService) =>
{
    try
    {
        var result = await accessControlService.GetRoleUsersAsync(roleId);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Failed to get role users", Details = ex.Message });
    }
})
.WithName("GetRoleUsers");

app.MapPost("/api/access/organizations", async (OrganizationRequest request, IAccessControlService accessControlService) =>
{
    try
    {
        var result = await accessControlService.CreateOrganizationAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Organization creation failed", Details = ex.Message });
    }
})
.WithName("CreateOrganization");

app.MapPut("/api/access/organizations/{organizationId}", async (string organizationId, OrganizationRequest request, IAccessControlService accessControlService) =>
{
    try
    {
        var result = await accessControlService.UpdateOrganizationAsync(organizationId, request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Organization update failed", Details = ex.Message });
    }
})
.WithName("UpdateOrganization");

app.MapDelete("/api/access/organizations/{organizationId}", async (string organizationId, IAccessControlService accessControlService) =>
{
    try
    {
        var result = await accessControlService.DeleteOrganizationAsync(organizationId);
        if (result)
        {
            return Results.Ok(new { Message = "Organization deleted successfully" });
        }
        else
        {
            return Results.NotFound();
        }
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Organization deletion failed", Details = ex.Message });
    }
})
.WithName("DeleteOrganization");

app.MapGet("/api/access/organizations", async (IAccessControlService accessControlService) =>
{
    try
    {
        var result = await accessControlService.GetOrganizationsAsync();
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Failed to get organizations", Details = ex.Message });
    }
})
.WithName("GetOrganizations");

app.MapGet("/api/access/organizations/{organizationId}", async (string organizationId, IAccessControlService accessControlService) =>
{
    try
    {
        var result = await accessControlService.GetOrganizationAsync(organizationId);
        if (result == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Failed to get organization", Details = ex.Message });
    }
})
.WithName("GetOrganization");

app.Run(); 