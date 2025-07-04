using System.ComponentModel.DataAnnotations;

namespace HardLock.AccessControl.Models;

public class PermissionRequest
{
    [Required]
    public string ResourceId { get; set; } = string.Empty;
    
    [Required]
    public string ResourceType { get; set; } = string.Empty; // file, folder, etc.
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string Permission { get; set; } = string.Empty; // read, write, delete, admin
    
    public DateTime? ExpiresAt { get; set; }
    
    public Dictionary<string, string>? Conditions { get; set; } // geo-fencing, time-based, etc.
}

public class PermissionResponse
{
    public string Id { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Permission { get; set; } = string.Empty;
    public DateTime GrantedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public Dictionary<string, string>? Conditions { get; set; }
}

public class AccessCheckRequest
{
    [Required]
    public string ResourceId { get; set; } = string.Empty;
    
    [Required]
    public string ResourceType { get; set; } = string.Empty;
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string RequiredPermission { get; set; } = string.Empty;
    
    public Dictionary<string, string>? Context { get; set; } // location, time, device, etc.
}

public class AccessCheckResponse
{
    public bool HasAccess { get; set; }
    public string Reason { get; set; } = string.Empty;
    public List<string> GrantedPermissions { get; set; } = new();
    public Dictionary<string, string>? Conditions { get; set; }
}

public class RoleRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public List<string> Permissions { get; set; } = new();
    
    public string? OrganizationId { get; set; }
}

public class RoleResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public string? OrganizationId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UserRoleRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string RoleId { get; set; } = string.Empty;
    
    public DateTime? ExpiresAt { get; set; }
}

public class UserRoleResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}

public class OrganizationRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string? ParentOrganizationId { get; set; }
}

public class OrganizationResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ParentOrganizationId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 