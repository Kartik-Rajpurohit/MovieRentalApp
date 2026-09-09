using System.Text.Json.Serialization;

namespace MovieRental.Domain.DTOs.Auth;

// Response DTO returned to client after successful login or registration
public class AuthResponseDto
{
    // Unique ID of the authenticated user
    public int UserId { get; set; }

    // User's email address
    public string Email { get; set; } = string.Empty;

    // User's display full name
    public string FullName { get; set; } = string.Empty;

    // Assigned role name (e.g. "Admin", "Staff", "Customer")
    public string Role { get; set; } = string.Empty;

    // Linked customer ID if the user is a customer
    public int? CustomerId { get; set; }

    // Linked store ID if user is associated with a specific store
    public int? StoreId { get; set; }

    // Linked staff ID if user is an employee
    public int? StaffId { get; set; }

    // Short-lived JWT access token for authenticating subsequent requests
    public string Token { get; set; } = string.Empty;

    // Sent as HttpOnly cookie — never exposed in the JSON response body
    [JsonIgnore]
    public string RefreshToken { get; set; } = string.Empty;
}