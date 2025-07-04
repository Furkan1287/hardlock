using HardLock.AccessControl.Models;

namespace HardLock.AccessControl.Services;

public interface IAccessControlService
{
    // Permission Management
    Task<PermissionResponse> GrantPermissionAsync(PermissionRequest request);
    Task<bool> RevokePermissionAsync(string permissionId);
    Task<List<PermissionResponse>> GetUserPermissionsAsync(string userId, string? resourceType = null);
    Task<List<PermissionResponse>> GetResourcePermissionsAsync(string resourceId, string resourceType);
    
    // Access Control
    Task<AccessCheckResponse> CheckAccessAsync(AccessCheckRequest request);
    Task<bool> HasPermissionAsync(string userId, string resourceId, string resourceType, string permission);
    
    // Role Management
    Task<RoleResponse> CreateRoleAsync(RoleRequest request);
    Task<RoleResponse> UpdateRoleAsync(string roleId, RoleRequest request);
    Task<bool> DeleteRoleAsync(string roleId);
    Task<List<RoleResponse>> GetRolesAsync(string? organizationId = null);
    Task<RoleResponse> GetRoleAsync(string roleId);
    
    // User Role Management
    Task<UserRoleResponse> AssignRoleToUserAsync(UserRoleRequest request);
    Task<bool> RemoveRoleFromUserAsync(string userRoleId);
    Task<List<UserRoleResponse>> GetUserRolesAsync(string userId);
    Task<List<UserRoleResponse>> GetRoleUsersAsync(string roleId);
    
    // Organization Management
    Task<OrganizationResponse> CreateOrganizationAsync(OrganizationRequest request);
    Task<OrganizationResponse> UpdateOrganizationAsync(string organizationId, OrganizationRequest request);
    Task<bool> DeleteOrganizationAsync(string organizationId);
    Task<List<OrganizationResponse>> GetOrganizationsAsync();
    Task<OrganizationResponse> GetOrganizationAsync(string organizationId);
} 