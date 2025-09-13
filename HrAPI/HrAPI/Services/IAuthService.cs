using HrAPI.DTOs;

namespace HrAPI.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<string> GenerateJwtTokenAsync(int userId, string email, string role);
    Task<bool> ValidateTokenAsync(string token);
}
