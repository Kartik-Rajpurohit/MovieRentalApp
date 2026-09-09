using MovieRental.Domain.DTOs.Auth;

namespace MovieRental.Services.Interfaces;

// Defines authentication operations: sign up, login, token refresh, and logout.
public interface IAuthService
{
    // Verifies credentials and generates JWT access and refresh tokens.
    Task<AuthResponseDto> LoginAsync(LoginDto dto);

    // Registers a new user and customer account, returning tokens.
    Task<AuthResponseDto> SignUpAsync(SignUpDto dto);

    // Validates a refresh token and generates a new access token pair.
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);

    // Revokes the given refresh token in the database upon user logout.
    Task LogoutAsync(string refreshToken, int? userId = null);

    // Revokes active tokens directly using the authenticated user's ID.
    Task LogoutByUserIdAsync(int userId);
}