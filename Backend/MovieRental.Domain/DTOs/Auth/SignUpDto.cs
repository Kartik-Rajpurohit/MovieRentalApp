using System.ComponentModel.DataAnnotations;

namespace MovieRental.Domain.DTOs.Auth;

// Request DTO containing registration data sent by the client to register a new user/customer
public class SignUpDto
{
    // First name of the registering user
    [Required(ErrorMessage = "First name is required")]
    [MaxLength(50, ErrorMessage = "First name must not exceed 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    // Last name of the registering user
    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(50, ErrorMessage = "Last name must not exceed 50 characters")]
    public string LastName { get; set; } = string.Empty;

    // Login email address for the new account
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    // Account password (must meet complexity requirements)
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [MaxLength(100, ErrorMessage = "Password must not exceed 100 characters")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$", ErrorMessage = "Password must contain at least one uppercase letter, one number, and one special character")]
    public string Password { get; set; } = string.Empty;

    // Street address line
    [Required(ErrorMessage = "Street address is required")]
    [MaxLength(50, ErrorMessage = "Street address must not exceed 50 characters")]
    public string Street { get; set; } = string.Empty;

    // City name
    [Required(ErrorMessage = "City is required")]
    [MaxLength(50, ErrorMessage = "City must not exceed 50 characters")]
    public string City { get; set; } = string.Empty;

    // Country name
    [Required(ErrorMessage = "Country is required")]
    [MaxLength(50, ErrorMessage = "Country must not exceed 50 characters")]
    public string Country { get; set; } = string.Empty;

    // Postal / zip code (optional per database schema)
    [MaxLength(10, ErrorMessage = "Postal code must not exceed 10 characters")]
    public string? PostalCode { get; set; }

    // Contact telephone number
    [Required(ErrorMessage = "Phone number is required")]
    [MaxLength(20, ErrorMessage = "Phone must not exceed 20 characters")]
    public string Phone { get; set; } = string.Empty;
}