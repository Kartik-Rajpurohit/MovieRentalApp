using System.ComponentModel.DataAnnotations;

namespace MovieRental.Domain.DTOs.Auth;

// Request DTO containing user login credentials sent by the client
public class LoginDto
{
    // User's registered email address
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    // User's account password in plain text (will be verified against hashed password)
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [MaxLength(100, ErrorMessage = "Password must not exceed 100 characters")]
    public string Password { get; set; } = string.Empty;
}