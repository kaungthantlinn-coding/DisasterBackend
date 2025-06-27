using DisasterApp.Application.DTOs;

namespace DisasterApp.Application.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> SignupAsync(SignupRequestDto request);
    Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<bool> LogoutAsync(string refreshToken);
    Task<bool> ValidateTokenAsync(string token);
}