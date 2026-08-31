namespace MovieRental.Domain.DTOs.Auth;

public class SignUpDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // Address — user can select existing or provide new
    public int? ExistingAddressId { get; set; }  // if user selects from suggestions
    public string? Street { get; set; }           // if user types new address
    public string? District { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public int? CityId { get; set; }              // required for new address
}