using Microsoft.AspNetCore.Mvc;
using HrAPI.DTOs;
using HrAPI.Services;

namespace HrAPI.Controllers;

/// <summary>
/// Controller for authentication operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration request containing user details</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.RegisterAsync(request);
            
            _logger.LogInformation("User {Email} registered successfully", request.Email);
            
            return CreatedAtAction(nameof(Register), new { email = request.Email }, response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Registration failed for {Email}: {Error}", request.Email, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for {Email}", request.Email);
            return StatusCode(500, new { message = "An unexpected error occurred during registration" });
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="request">Login request containing credentials</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.LoginAsync(request);
            
            _logger.LogInformation("User {Email} logged in successfully", request.Email);
            
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login failed for {Email}: {Error}", request.Email, ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for {Email}", request.Email);
            return StatusCode(500, new { message = "An unexpected error occurred during login" });
        }
    }

    /// <summary>
    /// Validate JWT token
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>Validation result</returns>
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateToken([FromBody] string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token is required" });
            }

            var isValid = await _authService.ValidateTokenAsync(token);
            
            return Ok(new { isValid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return StatusCode(500, new { message = "An error occurred while validating the token" });
        }
    }
}
