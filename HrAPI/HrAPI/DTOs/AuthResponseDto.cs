using HrAPI.Models;

namespace HrAPI.DTOs;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public UserDto User { get; set; } = new();
}

public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Department { get; set; }
    public string? Team { get; set; }
    public string? Description { get; set; }
    public string? Position { get; set; }
    public DateTime? HireDate { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public EmployeeRole Role { get; set; }
    public string FullName { get; set; } = string.Empty;
}
