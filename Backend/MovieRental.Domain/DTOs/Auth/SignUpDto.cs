using System.ComponentModel.DataAnnotations;

namespace MovieRental.Domain.DTOs.Auth;

public class SignUpDto
{
    [Required(ErrorMessage = "First name is required")]
    [MaxLength(50, ErrorMessage = "First name must not exceed 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(50, ErrorMessage = "Last name must not exceed 50 characters")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [MaxLength(100, ErrorMessage = "Password must not exceed 100 characters")]
    public string Password { get; set; } = string.Empty;

    // Address — user can select existing or provide new
    public int? ExistingAddressId { get; set; }  // if user selects from suggestions

    [MaxLength(255, ErrorMessage = "Street must not exceed 255 characters")]
    public string? Street { get; set; }           // if user types new address

    [MaxLength(100, ErrorMessage = "District must not exceed 100 characters")]
    public string? District { get; set; }

    [MaxLength(20, ErrorMessage = "Postal code must not exceed 20 characters")]
    public string? PostalCode { get; set; }

    [MaxLength(20, ErrorMessage = "Phone must not exceed 20 characters")]
    public string? Phone { get; set; }

    public int? CityId { get; set; }              // required for new address
}