using System.ComponentModel.DataAnnotations;

namespace MovieRental.Domain.DTOs.Auth;

// Request DTO sent by the client to obtain a new JWT access token using a refresh token
public class RefreshTokenDto
{
    // The refresh token previously issued to the client
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}