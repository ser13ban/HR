using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HrAPI.DTOs;
using HrAPI.Services;

namespace HrAPI.Controllers;

/// <summary>
/// Controller for employee operations
/// </summary>
[Route("api/[controller]")]
[Authorize]
public class EmployeeController : BaseController
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    /// <summary>
    /// Get all employees
    /// </summary>
    /// <returns>List of all employees</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeListDto>>> GetAllEmployees()
    {
        try
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees list");
            return StatusCode(500, new { message = "An error occurred while retrieving employees" });
        }
    }

    /// <summary>
    /// Get employee by ID
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <returns>Employee profile</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeProfileDto>> GetEmployeeById(string id)
    {
        try
        {
            // Get the requesting user's ID
            var currentUserId = GetCurrentUserId();

            var employee = await _employeeService.GetEmployeeByIdAsync(id, currentUserId);
            if (employee == null)
            {
                return NotFound(new { message = "Employee not found" });
            }

            return Ok(employee);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid employee ID format: {EmployeeId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access attempt for employee ID: {EmployeeId}", id);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee profile for ID: {EmployeeId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the employee profile" });
        }
    }


    /// <summary>
    /// Update employee profile
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="updateDto">Updated employee data</param>
    /// <returns>Updated employee profile</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<EmployeeProfileDto>> UpdateEmployee(string id, [FromBody] UpdateEmployeeDto updateDto)
    {
        try
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the requesting user's ID
            var currentUserId = GetCurrentUserId();

            var updatedEmployee = await _employeeService.UpdateEmployeeAsync(id, updateDto, currentUserId);
            if (updatedEmployee == null)
            {
                return NotFound(new { message = "Employee not found" });
            }

            return Ok(updatedEmployee);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid employee ID format: {EmployeeId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized update attempt for employee ID: {EmployeeId}", id);
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error updating employee profile for ID: {EmployeeId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating employee profile for ID: {EmployeeId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the employee profile" });
        }
    }
}
