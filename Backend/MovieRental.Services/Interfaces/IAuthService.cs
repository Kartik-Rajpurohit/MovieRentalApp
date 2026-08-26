using MovieRental.Domain.DTOs.Auth;

namespace MovieRental.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> SignUpAsync(SignUpDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
    Task LogoutAsync(string refreshToken);
}