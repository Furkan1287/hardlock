using System.ComponentModel.DataAnnotations;

namespace HardLock.Identity.Models;

public class UserLoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
    
    public string? MfaCode { get; set; }
} 