using System.ComponentModel.DataAnnotations;

namespace HrAPI.DTOs;

public class UpdateEmployeeDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(20)]
    [Phone]
    public string? PhoneNumber { get; set; }
    
    [MaxLength(100)]
    public string? Department { get; set; }
    
    [MaxLength(100)]
    public string? Team { get; set; }
    
    [MaxLength(100)]
    public string? Position { get; set; }
    
    [MaxLength(500)]
    public string? Bio { get; set; }
    
    [MaxLength(255)]
    public string? ProfilePictureUrl { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    [MaxLength(500)]
    public string? Address { get; set; }
    
    [MaxLength(100)]
    public string? EmergencyContact { get; set; }
    
    [MaxLength(20)]
    [Phone]
    public string? EmergencyPhone { get; set; }
}
