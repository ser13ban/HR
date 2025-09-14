using HrAPI.DTOs;
using HrAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrAPI.Controllers;

[Route("api/[controller]")]
[Authorize]
public class AbsenceController : BaseController
{
    private readonly IAbsenceService _absenceService;

    public AbsenceController(IAbsenceService absenceService)
    {
        _absenceService = absenceService;
    }

    [HttpGet("my-requests")]
    public async Task<ActionResult<IEnumerable<AbsenceRequestDto>>> GetMyAbsenceRequests()
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var requests = await _absenceService.GetMyAbsenceRequestsAsync(employeeId);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<IEnumerable<AbsenceRequestDto>>> GetEmployeeAbsenceRequests(int employeeId)
    {
        try
        {
            var requestingEmployeeId = GetCurrentEmployeeId();
            var requests = await _absenceService.GetEmployeeAbsenceRequestsAsync(employeeId, requestingEmployeeId);
            return Ok(requests);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("approved")]
    public async Task<ActionResult<IEnumerable<AbsenceRequestDto>>> GetApprovedAbsenceRequests()
    {
        try
        {
            var requests = await _absenceService.GetApprovedAbsenceRequestsAsync();
            return Ok(requests);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("pending-approvals")]
    public async Task<ActionResult<IEnumerable<AbsenceRequestDto>>> GetPendingApprovalsForManager()
    {
        try
        {
            var managerId = GetCurrentEmployeeId();
            var requests = await _absenceService.GetPendingApprovalsForManagerAsync(managerId);
            return Ok(requests);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<AbsenceRequestDto>> CreateAbsenceRequest([FromBody] CreateAbsenceRequestDto createDto)
    {
        try
        {
            if (createDto == null)
            {
                return BadRequest(new { message = "Request body is required." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(new { message = "Validation failed.", errors });
            }

            var employeeId = GetCurrentEmployeeId();
            var request = await _absenceService.CreateAbsenceRequestAsync(employeeId, createDto);
            return CreatedAtAction(nameof(GetMyAbsenceRequests), new { id = request.Id }, request);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the absence request.", details = ex.Message });
        }
    }

    [HttpPut("{id}/approve")]
    public async Task<ActionResult<AbsenceRequestDto>> ApproveAbsenceRequest(int id, [FromBody] ApprovalActionDto approvalDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var approverId = GetCurrentEmployeeId();
            var request = await _absenceService.ApproveAbsenceRequestAsync(id, approverId, approvalDto);
            return Ok(request);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while approving the absence request." });
        }
    }

    [HttpPut("{id}/decline")]
    public async Task<ActionResult<AbsenceRequestDto>> DeclineAbsenceRequest(int id, [FromBody] ApprovalActionDto approvalDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var approverId = GetCurrentEmployeeId();
            var request = await _absenceService.DeclineAbsenceRequestAsync(id, approverId, approvalDto);
            return Ok(request);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while declining the absence request." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> CancelAbsenceRequest(int id)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var success = await _absenceService.CancelAbsenceRequestAsync(id, employeeId);
            
            if (!success)
            {
                return NotFound(new { message = "Absence request not found or you don't have permission to cancel it." });
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while cancelling the absence request." });
        }
    }

}
