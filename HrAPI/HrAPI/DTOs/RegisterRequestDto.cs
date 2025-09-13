using System.ComponentModel.DataAnnotations;
using HrAPI.Models;

namespace HrAPI.DTOs;

public class RegisterRequestDto
{
    [Required(ErrorMessage = "First name is required")]
    [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required")]
    public EmployeeRole Role { get; set; }

    [MaxLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
    public string? Department { get; set; }

    [MaxLength(100, ErrorMessage = "Team cannot exceed 100 characters")]
    public string? Team { get; set; }

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
}
