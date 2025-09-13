using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HrAPI.DTOs;
using HrAPI.Models;

namespace HrAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly UserManager<Employee> _userManager;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(UserManager<Employee> userManager, ILogger<EmployeeController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeListDto>>> GetAllEmployees()
    {
        try
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
                    Role = e.Role.ToString()
                })
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees list");
            return StatusCode(500, "An error occurred while retrieving employees");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeProfileDto>> GetEmployeeById(string id)
    {
        try
        {
            // Parse the employee ID
            if (!int.TryParse(id, out var employeeId))
            {
                return BadRequest("Invalid employee ID format");
            }

            // Get the requesting user's ID and role
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                return Unauthorized("Invalid user token");
            }

            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var currentUser = await _userManager.FindByIdAsync(currentUserId.ToString());
            
            if (currentUser == null)
            {
                return Unauthorized("User not found");
            }

            // Get the requested employee
            var employee = await _userManager.FindByIdAsync(employeeId.ToString());
            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            // Determine if this is a limited view
            var isLimitedView = DetermineIfLimitedView(currentUserId, employeeId, currentUser.Role);

            // Map to DTO based on view permissions
            var profileDto = MapToProfileDto(employee, isLimitedView);

            return Ok(profileDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee profile for ID: {EmployeeId}", id);
            return StatusCode(500, "An error occurred while retrieving the employee profile");
        }
    }

    private static bool DetermineIfLimitedView(int currentUserId, int requestedEmployeeId, EmployeeRole currentUserRole)
    {
        // If viewing own profile, full access
        if (currentUserId == requestedEmployeeId)
            return false;

        // If current user is Manager or Admin, full access to all profiles
        if (currentUserRole == EmployeeRole.Manager || currentUserRole == EmployeeRole.Admin)
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
            Role = employee.Role.ToString(),
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

    [HttpPut("{id}")]
    public async Task<ActionResult<EmployeeProfileDto>> UpdateEmployee(string id, [FromBody] UpdateEmployeeDto updateDto)
    {
        try
        {
            // Parse the employee ID
            if (!int.TryParse(id, out var employeeId))
            {
                return BadRequest("Invalid employee ID format");
            }

            // Get the requesting user's ID and role
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                return Unauthorized("Invalid user token");
            }

            var currentUser = await _userManager.FindByIdAsync(currentUserId.ToString());
            if (currentUser == null)
            {
                return Unauthorized("User not found");
            }

            // Check permissions: can edit own profile OR manager/admin can edit any profile
            var canEdit = currentUserId == employeeId || 
                         currentUser.Role == EmployeeRole.Manager || 
                         currentUser.Role == EmployeeRole.Admin;

            if (!canEdit)
            {
                return Forbid("You don't have permission to edit this profile");
            }

            // Get the employee to update
            var employee = await _userManager.FindByIdAsync(employeeId.ToString());
            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            // Validate model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
                return BadRequest($"Failed to update employee: {errors}");
            }

            // Return updated profile with appropriate permissions
            var isLimitedView = DetermineIfLimitedView(currentUserId, employeeId, currentUser.Role);
            var updatedDto = MapToProfileDto(employee, isLimitedView);

            return Ok(updatedDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee profile for ID: {EmployeeId}", id);
            return StatusCode(500, "An error occurred while updating the employee profile");
        }
    }
}
