using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace HrAPI.Models;

public class Employee : IdentityUser<int>
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    [MaxLength(100)]
    public string? Department { get; set; }
    
    [MaxLength(100)]
    public string? Team { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [MaxLength(100)]
    public string? Position { get; set; }
    
    public DateTime? HireDate { get; set; }
    
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
    public string? EmergencyPhone { get; set; }
    
    public EmployeeRole Role { get; set; } = EmployeeRole.Employee;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<AbsenceRequest> AbsenceRequests { get; set; } = new List<AbsenceRequest>();
    public virtual ICollection<Feedback> GivenFeedback { get; set; } = new List<Feedback>();
    public virtual ICollection<Feedback> ReceivedFeedback { get; set; } = new List<Feedback>();
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
}

public enum EmployeeRole
{
    Employee = 0,
    Manager = 1,
    Admin = 2
}
