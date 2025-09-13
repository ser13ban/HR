namespace HrAPI.DTOs;

public class EmployeeProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Team { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    
    // Indicates if this is a limited view (for co-workers)
    public bool IsLimitedView { get; set; }
}
