using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using HrAPI.DTOs;
using HrAPI.Models;

namespace HrAPI.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<Employee> _userManager;
    private readonly SignInManager<Employee> _signInManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<Employee> userManager,
        SignInManager<Employee> signInManager,
        RoleManager<IdentityRole<int>> roleManager,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            // Create new employee
            var employee = new Employee
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Department = request.Department,
                Team = request.Team,
                Description = request.Description,
                Role = request.Role,
                EmailConfirmed = true // For simplicity, auto-confirm email
            };

            // Create user with Identity
            var result = await _userManager.CreateAsync(employee, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }

            // Assign role
            var roleName = request.Role.ToString();
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole<int> { Name = roleName });
            }

            await _userManager.AddToRoleAsync(employee, roleName);

            // Generate JWT token
            var token = await GenerateJwtTokenAsync(employee.Id, employee.Email!, roleName);
            var expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpireMinutes"));

            return new AuthResponseDto
            {
                Token = token,
                Expires = expires,
                User = MapToUserDto(employee)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during user registration");
            throw;
        }
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        try
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Check password
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? "Employee";

            // Generate JWT token
            var token = await GenerateJwtTokenAsync(user.Id, user.Email!, primaryRole);
            var expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpireMinutes"));

            return new AuthResponseDto
            {
                Token = token,
                Expires = expires,
                User = MapToUserDto(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during user login");
            throw;
        }
    }

    public async Task<string> GenerateJwtTokenAsync(int userId, string email, string role)
    {
        var jwtKey = _configuration["Jwt:Key"];
        var jwtIssuer = _configuration["Jwt:Issuer"];
        var jwtAudience = _configuration["Jwt:Audience"];
        var expireMinutes = _configuration.GetValue<int>("Jwt:ExpireMinutes");

        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new InvalidOperationException("JWT key is not configured");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey))
            {
                return false;
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            await Task.Run(() => tokenHandler.ValidateToken(token, validationParameters, out _));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static UserDto MapToUserDto(Employee employee)
    {
        return new UserDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email!,
            PhoneNumber = employee.PhoneNumber,
            Department = employee.Department,
            Team = employee.Team,
            Description = employee.Description,
            Position = employee.Position,
            HireDate = employee.HireDate,
            Bio = employee.Bio,
            ProfilePictureUrl = employee.ProfilePictureUrl,
            Role = employee.Role,
            FullName = employee.FullName
        };
    }
}
