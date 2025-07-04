using System.ComponentModel.DataAnnotations;

namespace HardLock.Identity.Models;

public class UserUpdateRequest
{
    [StringLength(100)]
    public string? FirstName { get; set; }
    
    [StringLength(100)]
    public string? LastName { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }
    
    public bool? IsActive { get; set; }
} 