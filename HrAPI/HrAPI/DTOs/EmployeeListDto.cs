namespace HrAPI.DTOs;

public class EmployeeListDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Team { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string Role { get; set; } = string.Empty;
}
