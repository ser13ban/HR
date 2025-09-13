using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HrAPI.Services;
using HrAPI.DTOs;

namespace HrAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;

    public FeedbackController(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [HttpGet("received/{employeeId}")]
    public async Task<ActionResult<IEnumerable<FeedbackListDto>>> GetReceivedFeedback(int employeeId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var feedbacks = await _feedbackService.GetReceivedFeedbackAsync(employeeId, userId);
            return Ok(feedbacks);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("given/{employeeId}")]
    public async Task<ActionResult<IEnumerable<FeedbackListDto>>> GetGivenFeedback(int employeeId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var feedbacks = await _feedbackService.GetGivenFeedbackAsync(employeeId, userId);
            return Ok(feedbacks);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FeedbackDetailDto>> GetFeedback(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id, userId);
            
            if (feedback == null)
            {
                return NotFound("Feedback not found.");
            }

            return Ok(feedback);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<FeedbackDetailDto>> CreateFeedback([FromBody] CreateFeedbackDto createFeedbackDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetCurrentUserId();
            var feedback = await _feedbackService.CreateFeedbackAsync(createFeedbackDto, userId);
            return CreatedAtAction(nameof(GetFeedback), new { id = feedback.Id }, feedback);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("can-view/{employeeId}")]
    public async Task<ActionResult<bool>> CanViewFeedback(int employeeId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var canView = await _feedbackService.CanUserViewFeedbackAsync(employeeId, userId);
            return Ok(canView);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("can-give/{employeeId}")]
    public async Task<ActionResult<bool>> CanGiveFeedback(int employeeId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var canGive = await _feedbackService.CanUserGiveFeedbackAsync(employeeId, userId);
            return Ok(canGive);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("User ID not found in token.");
    }
}
