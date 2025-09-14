using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HrAPI.Controllers;

/// <summary>
/// Base controller providing common functionality for all API controllers
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Gets the current user's ID from the JWT token claims
    /// </summary>
    /// <returns>The current user's ID</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user ID is not found in token or is invalid</exception>
    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("User ID not found in token.");
    }

    /// <summary>
    /// Gets the current employee's ID from the JWT token claims
    /// Alias for GetCurrentUserId() for better semantic meaning in employee contexts
    /// </summary>
    /// <returns>The current employee's ID</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when employee ID is not found in token or is invalid</exception>
    protected int GetCurrentEmployeeId()
    {
        return GetCurrentUserId();
    }
}
