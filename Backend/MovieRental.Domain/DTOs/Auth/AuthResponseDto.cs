using System.Text.Json.Serialization;

namespace MovieRental.Domain.DTOs.Auth;

public class AuthResponseDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public int? StoreId { get; set; }
    public int? StaffId { get; set; }
    public string Token { get; set; } = string.Empty;

    // Sent as HttpOnly cookie — never exposed in the JSON response body
    [JsonIgnore]
    public string RefreshToken { get; set; } = string.Empty;
}