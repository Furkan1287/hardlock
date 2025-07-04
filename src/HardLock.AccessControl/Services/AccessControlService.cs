using HardLock.AccessControl.Models;
using HardLock.AccessControl.Data;
using HardLock.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HardLock.AccessControl.Services;

public class AccessControlService : IAccessControlService
{
    private readonly AccessControlDbContext _context;
    private readonly Microsoft.Extensions.Logging.ILogger<AccessControlService> _logger;

    public AccessControlService(AccessControlDbContext context, Microsoft.Extensions.Logging.ILogger<AccessControlService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PermissionResponse> GrantPermissionAsync(PermissionRequest request)
    {
        try
        {
            var permission = new Permission
            {
                Id = Guid.NewGuid(),
                ResourceId = request.ResourceId,
                ResourceType = request.ResourceType,
                UserId = request.UserId,
                PermissionType = request.Permission,
                GrantedAt = DateTime.UtcNow,
                ExpiresAt = request.ExpiresAt,
                IsActive = true,
                Conditions = request.Conditions != null ? JsonSerializer.Serialize(request.Conditions) : null
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Permission granted: {UserId} -> {ResourceId} ({Permission})", 
                request.UserId, request.ResourceId, request.Permission);

            return new PermissionResponse
            {
                Id = permission.Id.ToString(),
                ResourceId = permission.ResourceId,
                ResourceType = permission.ResourceType,
                UserId = permission.UserId,
                Permission = permission.PermissionType,
                GrantedAt = permission.GrantedAt,
                ExpiresAt = permission.ExpiresAt,
                IsActive = permission.IsActive,
                Conditions = permission.Conditions != null ? JsonSerializer.Deserialize<Dictionary<string, string>>(permission.Conditions) : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error granting permission");
            throw;
        }
    }

    public async Task<bool> RevokePermissionAsync(string permissionId)
    {
        try
        {
            if (!Guid.TryParse(permissionId, out var id))
            {
                return false;
            }

            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
            {
                return false;
            }

            permission.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Permission revoked: {PermissionId}", permissionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking permission: {PermissionId}", permissionId);
            return false;
        }
    }

    public async Task<List<PermissionResponse>> GetUserPermissionsAsync(string userId, string? resourceType = null)
    {
        try
        {
            var query = _context.Permissions
                .Where(p => p.UserId == userId && p.IsActive);

            if (!string.IsNullOrEmpty(resourceType))
            {
                query = query.Where(p => p.ResourceType == resourceType);
            }

            var permissions = await query.ToListAsync();

            return permissions.Select(p => new PermissionResponse
            {
                Id = p.Id.ToString(),
                ResourceId = p.ResourceId,
                ResourceType = p.ResourceType,
                UserId = p.UserId,
                Permission = p.PermissionType,
                GrantedAt = p.GrantedAt,
                ExpiresAt = p.ExpiresAt,
                IsActive = p.IsActive,
                Conditions = p.Conditions != null ? JsonSerializer.Deserialize<Dictionary<string, string>>(p.Conditions) : null
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user permissions: {UserId}", userId);
            throw;
        }
    }

    public async Task<List<PermissionResponse>> GetResourcePermissionsAsync(string resourceId, string resourceType)
    {
        try
        {
            var permissions = await _context.Permissions
                .Where(p => p.ResourceId == resourceId && p.ResourceType == resourceType && p.IsActive)
                .ToListAsync();

            return permissions.Select(p => new PermissionResponse
            {
                Id = p.Id.ToString(),
                ResourceId = p.ResourceId,
                ResourceType = p.ResourceType,
                UserId = p.UserId,
                Permission = p.PermissionType,
                GrantedAt = p.GrantedAt,
                ExpiresAt = p.ExpiresAt,
                IsActive = p.IsActive,
                Conditions = p.Conditions != null ? JsonSerializer.Deserialize<Dictionary<string, string>>(p.Conditions) : null
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting resource permissions: {ResourceId}", resourceId);
            throw;
        }
    }

    public async Task<AccessCheckResponse> CheckAccessAsync(AccessCheckRequest request)
    {
        try
        {
            // Check direct permissions
            var directPermission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.ResourceId == request.ResourceId && 
                                        p.ResourceType == request.ResourceType && 
                                        p.UserId == request.UserId && 
                                        p.PermissionType == request.RequiredPermission && 
                                        p.IsActive &&
                                        (p.ExpiresAt == null || p.ExpiresAt > DateTime.UtcNow));

            if (directPermission != null)
            {
                return new AccessCheckResponse
                {
                    HasAccess = true,
                    Reason = "Direct permission granted",
                    GrantedPermissions = new List<string> { directPermission.PermissionType },
                    Conditions = directPermission.Conditions != null ? JsonSerializer.Deserialize<Dictionary<string, string>>(directPermission.Conditions) : null
                };
            }

            // Check role-based permissions
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == request.UserId && ur.IsActive)
                .ToListAsync();

            foreach (var userRole in userRoles)
            {
                var rolePermissions = await _context.RolePermissions
                    .Where(rp => rp.RoleId == userRole.RoleId)
                    .ToListAsync();

                if (rolePermissions.Any(rp => rp.Permission == request.RequiredPermission))
                {
                    return new AccessCheckResponse
                    {
                        HasAccess = true,
                        Reason = $"Role-based permission granted via role {userRole.RoleId}",
                        GrantedPermissions = rolePermissions.Select(rp => rp.Permission).ToList()
                    };
                }
            }

            return new AccessCheckResponse
            {
                HasAccess = false,
                Reason = "No permission found",
                GrantedPermissions = new List<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking access: {UserId} -> {ResourceId}", request.UserId, request.ResourceId);
            throw;
        }
    }

    public async Task<bool> HasPermissionAsync(string userId, string resourceId, string resourceType, string permission)
    {
        try
        {
            var result = await CheckAccessAsync(new AccessCheckRequest
            {
                UserId = userId,
                ResourceId = resourceId,
                ResourceType = resourceType,
                RequiredPermission = permission
            });

            return result.HasAccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission: {UserId} -> {ResourceId}", userId, resourceId);
            return false;
        }
    }

    public async Task<RoleResponse> CreateRoleAsync(RoleRequest request)
    {
        try
        {
            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                OrganizationId = request.OrganizationId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Roles.Add(role);

            // Add role permissions
            foreach (var permission in request.Permissions)
            {
                var rolePermission = new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = role.Id,
                    Permission = permission,
                    CreatedAt = DateTime.UtcNow
                };
                _context.RolePermissions.Add(rolePermission);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Role created: {RoleName}", request.Name);

            return new RoleResponse
            {
                Id = role.Id.ToString(),
                Name = role.Name,
                Description = role.Description,
                Permissions = request.Permissions,
                OrganizationId = role.OrganizationId,
                CreatedAt = role.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role: {RoleName}", request.Name);
            throw;
        }
    }

    public async Task<RoleResponse> UpdateRoleAsync(string roleId, RoleRequest request)
    {
        try
        {
            if (!Guid.TryParse(roleId, out var id))
            {
                throw new ArgumentException("Invalid role ID");
            }

            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                throw new KeyNotFoundException("Role not found");
            }

            role.Name = request.Name;
            role.Description = request.Description;
            role.OrganizationId = request.OrganizationId;
            role.UpdatedAt = DateTime.UtcNow;

            // Update role permissions
            var existingPermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == id)
                .ToListAsync();

            _context.RolePermissions.RemoveRange(existingPermissions);

            foreach (var permission in request.Permissions)
            {
                var rolePermission = new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = id,
                    Permission = permission,
                    CreatedAt = DateTime.UtcNow
                };
                _context.RolePermissions.Add(rolePermission);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Role updated: {RoleName}", request.Name);

            return new RoleResponse
            {
                Id = role.Id.ToString(),
                Name = role.Name,
                Description = role.Description,
                Permissions = request.Permissions,
                OrganizationId = role.OrganizationId,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role: {RoleId}", roleId);
            throw;
        }
    }

    public async Task<bool> DeleteRoleAsync(string roleId)
    {
        try
        {
            if (!Guid.TryParse(roleId, out var id))
            {
                return false;
            }

            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return false;
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Role deleted: {RoleId}", roleId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role: {RoleId}", roleId);
            return false;
        }
    }

    public async Task<List<RoleResponse>> GetRolesAsync(string? organizationId = null)
    {
        try
        {
            var query = _context.Roles.AsQueryable();

            if (!string.IsNullOrEmpty(organizationId))
            {
                query = query.Where(r => r.OrganizationId == organizationId);
            }

            var roles = await query.ToListAsync();

            return roles.Select(r => new RoleResponse
            {
                Id = r.Id.ToString(),
                Name = r.Name,
                Description = r.Description,
                OrganizationId = r.OrganizationId,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles");
            throw;
        }
    }

    public async Task<RoleResponse> GetRoleAsync(string roleId)
    {
        try
        {
            if (!Guid.TryParse(roleId, out var id))
            {
                throw new ArgumentException("Invalid role ID");
            }

            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                throw new KeyNotFoundException("Role not found");
            }

            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == id)
                .Select(rp => rp.Permission)
                .ToListAsync();

            return new RoleResponse
            {
                Id = role.Id.ToString(),
                Name = role.Name,
                Description = role.Description,
                Permissions = permissions,
                OrganizationId = role.OrganizationId,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role: {RoleId}", roleId);
            throw;
        }
    }

    public async Task<UserRoleResponse> AssignRoleToUserAsync(UserRoleRequest request)
    {
        try
        {
            if (!Guid.TryParse(request.RoleId, out var roleId))
            {
                throw new ArgumentException("Invalid role ID");
            }

            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
            {
                throw new KeyNotFoundException("Role not found");
            }

            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow,
                ExpiresAt = request.ExpiresAt,
                IsActive = true
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Role assigned to user: {UserId} -> {RoleId}", request.UserId, request.RoleId);

            return new UserRoleResponse
            {
                Id = userRole.Id.ToString(),
                UserId = userRole.UserId,
                RoleId = userRole.RoleId.ToString(),
                RoleName = role.Name,
                AssignedAt = userRole.AssignedAt,
                ExpiresAt = userRole.ExpiresAt,
                IsActive = userRole.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user: {UserId} -> {RoleId}", request.UserId, request.RoleId);
            throw;
        }
    }

    public async Task<bool> RemoveRoleFromUserAsync(string userRoleId)
    {
        try
        {
            if (!Guid.TryParse(userRoleId, out var id))
            {
                return false;
            }

            var userRole = await _context.UserRoles.FindAsync(id);
            if (userRole == null)
            {
                return false;
            }

            userRole.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Role removed from user: {UserRoleId}", userRoleId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role from user: {UserRoleId}", userRoleId);
            return false;
        }
    }

    public async Task<List<UserRoleResponse>> GetUserRolesAsync(string userId)
    {
        try
        {
            var userRoles = await _context.UserRoles
                .Include(ur => ur.RoleId)
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .ToListAsync();

            return userRoles.Select(ur => new UserRoleResponse
            {
                Id = ur.Id.ToString(),
                UserId = ur.UserId,
                RoleId = ur.RoleId.ToString(),
                RoleName = ur.RoleId.ToString(), // This should be the role name, but we need to join with Roles table
                AssignedAt = ur.AssignedAt,
                ExpiresAt = ur.ExpiresAt,
                IsActive = ur.IsActive
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user roles: {UserId}", userId);
            throw;
        }
    }

    public async Task<List<UserRoleResponse>> GetRoleUsersAsync(string roleId)
    {
        try
        {
            if (!Guid.TryParse(roleId, out var id))
            {
                throw new ArgumentException("Invalid role ID");
            }

            var userRoles = await _context.UserRoles
                .Where(ur => ur.RoleId == id && ur.IsActive)
                .ToListAsync();

            return userRoles.Select(ur => new UserRoleResponse
            {
                Id = ur.Id.ToString(),
                UserId = ur.UserId,
                RoleId = ur.RoleId.ToString(),
                RoleName = string.Empty, // Would need to join with Roles table
                AssignedAt = ur.AssignedAt,
                ExpiresAt = ur.ExpiresAt,
                IsActive = ur.IsActive
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role users: {RoleId}", roleId);
            throw;
        }
    }

    public async Task<OrganizationResponse> CreateOrganizationAsync(OrganizationRequest request)
    {
        try
        {
            var organization = new Organization
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                ParentOrganizationId = !string.IsNullOrEmpty(request.ParentOrganizationId) && 
                                     Guid.TryParse(request.ParentOrganizationId, out var parentId) ? parentId : null,
                CreatedAt = DateTime.UtcNow
            };

            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Organization created: {OrgName}", request.Name);

            return new OrganizationResponse
            {
                Id = organization.Id.ToString(),
                Name = organization.Name,
                Description = organization.Description,
                ParentOrganizationId = organization.ParentOrganizationId?.ToString(),
                CreatedAt = organization.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating organization: {OrgName}", request.Name);
            throw;
        }
    }

    public async Task<OrganizationResponse> UpdateOrganizationAsync(string organizationId, OrganizationRequest request)
    {
        try
        {
            if (!Guid.TryParse(organizationId, out var id))
            {
                throw new ArgumentException("Invalid organization ID");
            }

            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                throw new KeyNotFoundException("Organization not found");
            }

            organization.Name = request.Name;
            organization.Description = request.Description;
            organization.ParentOrganizationId = !string.IsNullOrEmpty(request.ParentOrganizationId) && 
                                             Guid.TryParse(request.ParentOrganizationId, out var parentId) ? parentId : null;
            organization.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Organization updated: {OrgName}", request.Name);

            return new OrganizationResponse
            {
                Id = organization.Id.ToString(),
                Name = organization.Name,
                Description = organization.Description,
                ParentOrganizationId = organization.ParentOrganizationId?.ToString(),
                CreatedAt = organization.CreatedAt,
                UpdatedAt = organization.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organization: {OrgId}", organizationId);
            throw;
        }
    }

    public async Task<bool> DeleteOrganizationAsync(string organizationId)
    {
        try
        {
            if (!Guid.TryParse(organizationId, out var id))
            {
                return false;
            }

            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return false;
            }

            _context.Organizations.Remove(organization);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Organization deleted: {OrgId}", organizationId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting organization: {OrgId}", organizationId);
            return false;
        }
    }

    public async Task<List<OrganizationResponse>> GetOrganizationsAsync()
    {
        try
        {
            var organizations = await _context.Organizations.ToListAsync();

            return organizations.Select(o => new OrganizationResponse
            {
                Id = o.Id.ToString(),
                Name = o.Name,
                Description = o.Description,
                ParentOrganizationId = o.ParentOrganizationId?.ToString(),
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organizations");
            throw;
        }
    }

    public async Task<OrganizationResponse> GetOrganizationAsync(string organizationId)
    {
        try
        {
            if (!Guid.TryParse(organizationId, out var id))
            {
                throw new ArgumentException("Invalid organization ID");
            }

            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                throw new KeyNotFoundException("Organization not found");
            }

            return new OrganizationResponse
            {
                Id = organization.Id.ToString(),
                Name = organization.Name,
                Description = organization.Description,
                ParentOrganizationId = organization.ParentOrganizationId?.ToString(),
                CreatedAt = organization.CreatedAt,
                UpdatedAt = organization.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organization: {OrgId}", organizationId);
            throw;
        }
    }
} 