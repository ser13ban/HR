using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HrAPI.DTOs;
using HrAPI.Models;

namespace HrAPI.Services;

public class EmployeeService : IEmployeeService
{
    private readonly UserManager<Employee> _userManager;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(UserManager<Employee> userManager, ILogger<EmployeeService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IEnumerable<EmployeeListDto>> GetAllEmployeesAsync()
    {
        var employees = await _userManager.Users
            .Where(e => !string.IsNullOrEmpty(e.FirstName) && !string.IsNullOrEmpty(e.LastName))
            .Select(e => new EmployeeListDto
            {
                Id = e.Id.ToString(),
                FirstName = e.FirstName,
                LastName = e.LastName,
                FullName = e.FirstName + " " + e.LastName,
                Department = e.Department ?? "Not Assigned",
                Team = e.Team ?? "Not Assigned",
                Position = e.Position ?? "Not Assigned",
                ProfilePictureUrl = e.ProfilePictureUrl,
                Role = e.Role
            })
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();

        return employees;
    }

    public async Task<EmployeeProfileDto?> GetEmployeeByIdAsync(string id, int requestingUserId)
    {
        // Parse the employee ID
        if (!int.TryParse(id, out var employeeId))
        {
            throw new ArgumentException("Invalid employee ID format", nameof(id));
        }

        // Get the requesting user
        var currentUser = await _userManager.FindByIdAsync(requestingUserId.ToString());
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        // Get the requested employee
        var employee = await _userManager.FindByIdAsync(employeeId.ToString());
        if (employee == null)
        {
            return null;
        }

        // Determine if this is a limited view
        var isLimitedView = DetermineIfLimitedView(requestingUserId, employeeId, currentUser.Role);

        // Map to DTO based on view permissions
        var profileDto = MapToProfileDto(employee, isLimitedView);

        return profileDto;
    }

    public async Task<EmployeeProfileDto?> UpdateEmployeeAsync(string id, UpdateEmployeeDto updateDto, int requestingUserId)
    {
        // Parse the employee ID
        if (!int.TryParse(id, out var employeeId))
        {
            throw new ArgumentException("Invalid employee ID format", nameof(id));
        }

        // Get the requesting user
        var currentUser = await _userManager.FindByIdAsync(requestingUserId.ToString());
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        // Check permissions: can edit own profile OR manager can edit any profile
        var canEdit = requestingUserId == employeeId || 
                     currentUser.Role == EmployeeRole.Manager;

        if (!canEdit)
        {
            throw new UnauthorizedAccessException("You don't have permission to edit this profile");
        }

        // Get the employee to update
        var employee = await _userManager.FindByIdAsync(employeeId.ToString());
        if (employee == null)
        {
            return null;
        }

        // Update employee properties (excluding sensitive fields like Role, CreatedAt)
        employee.FirstName = updateDto.FirstName;
        employee.LastName = updateDto.LastName;
        employee.Email = updateDto.Email;
        employee.UserName = updateDto.Email; // Keep UserName in sync with Email
        employee.PhoneNumber = updateDto.PhoneNumber;
        employee.Department = updateDto.Department;
        employee.Team = updateDto.Team;
        employee.Position = updateDto.Position;
        employee.Bio = updateDto.Bio;
        employee.ProfilePictureUrl = updateDto.ProfilePictureUrl;
        employee.DateOfBirth = updateDto.DateOfBirth;
        employee.Address = updateDto.Address;
        employee.EmergencyContact = updateDto.EmergencyContact;
        employee.EmergencyPhone = updateDto.EmergencyPhone;
        employee.UpdatedAt = DateTime.UtcNow;

        // Save changes
        var result = await _userManager.UpdateAsync(employee);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update employee: {errors}");
        }

        // Return updated profile with appropriate permissions
        var isLimitedView = DetermineIfLimitedView(requestingUserId, employeeId, currentUser.Role);
        var updatedDto = MapToProfileDto(employee, isLimitedView);

        return updatedDto;
    }

    private static bool DetermineIfLimitedView(int currentUserId, int requestedEmployeeId, EmployeeRole currentUserRole)
    {
        // If viewing own profile, full access
        if (currentUserId == requestedEmployeeId)
            return false;

        // If current user is Manager, full access to all profiles
        if (currentUserRole == EmployeeRole.Manager)
            return false;

        // Otherwise, limited view (co-worker accessing another's profile)
        return true;
    }

    private static EmployeeProfileDto MapToProfileDto(Employee employee, bool isLimitedView)
    {
        var dto = new EmployeeProfileDto
        {
            Id = employee.Id.ToString(),
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            FullName = employee.FullName,
            Department = employee.Department ?? "Not Assigned",
            Team = employee.Team ?? "Not Assigned",
            Position = employee.Position ?? "Not Assigned",
            ProfilePictureUrl = employee.ProfilePictureUrl,
            Role = employee.Role,
            IsLimitedView = isLimitedView
        };

        if (!isLimitedView)
        {
            // Full access - include all information
            dto.Email = employee.Email ?? string.Empty;
            dto.PhoneNumber = employee.PhoneNumber;
            dto.StartDate = employee.HireDate ?? employee.CreatedAt;
            dto.DateOfBirth = employee.DateOfBirth?.ToString("yyyy-MM-dd");
            dto.Address = employee.Address;
            dto.EmergencyContact = employee.EmergencyContact;
            dto.EmergencyPhone = employee.EmergencyPhone;
        }
        else
        {
            // Limited access - exclude sensitive information
            dto.Email = string.Empty;
            dto.PhoneNumber = null;
            dto.StartDate = employee.HireDate ?? employee.CreatedAt;
            dto.DateOfBirth = null;
            dto.Address = null;
            dto.EmergencyContact = null;
            dto.EmergencyPhone = null;
        }

        return dto;
    }
}
