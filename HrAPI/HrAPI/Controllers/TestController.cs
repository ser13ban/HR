using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HrAPI.Data;
using HrAPI.Models;

namespace HrAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly HrDbContext _context;

    public TestController(HrDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all employees with their related data
    /// </summary>
    /// <returns>A list of all employees including absence requests and feedback</returns>
    /// <response code="200">Returns the list of employees</response>
    [HttpGet("employees")]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
    {
        var employees = await _context.Employees
            .Include(e => e.AbsenceRequests)
            .Include(e => e.GivenFeedback)
            .Include(e => e.ReceivedFeedback)
            .ToListAsync();

        return Ok(employees);
    }

    /// <summary>
    /// Retrieves a specific employee by ID
    /// </summary>
    /// <param name="id">The employee ID</param>
    /// <returns>The employee with related data</returns>
    /// <response code="200">Returns the requested employee</response>
    /// <response code="404">If the employee is not found</response>
    [HttpGet("employees/{id}")]
    public async Task<ActionResult<Employee>> GetEmployee(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.AbsenceRequests)
            .Include(e => e.GivenFeedback)
            .Include(e => e.ReceivedFeedback)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
        {
            return NotFound();
        }

        return Ok(employee);
    }

    /// <summary>
    /// Retrieves all absence requests with related employee data
    /// </summary>
    /// <returns>A list of all absence requests</returns>
    /// <response code="200">Returns the list of absence requests</response>
    [HttpGet("absence-requests")]
    public async Task<ActionResult<IEnumerable<AbsenceRequest>>> GetAbsenceRequests()
    {
        var requests = await _context.AbsenceRequests
            .Include(a => a.Employee)
            .Include(a => a.ApprovedBy)
            .ToListAsync();

        return Ok(requests);
    }

    /// <summary>
    /// Retrieves all feedback records with related employee data
    /// </summary>
    /// <returns>A list of all feedback records</returns>
    /// <response code="200">Returns the list of feedback records</response>
    [HttpGet("feedbacks")]
    public async Task<ActionResult<IEnumerable<Feedback>>> GetFeedbacks()
    {
        var feedbacks = await _context.Feedbacks
            .Include(f => f.FromEmployee)
            .Include(f => f.ToEmployee)
            .ToListAsync();

        return Ok(feedbacks);
    }
}
