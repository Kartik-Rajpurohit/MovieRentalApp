namespace MovieRental.Services.Interfaces;

// Provides a clean abstraction over the current authenticated user's JWT claims.
// Replaces direct IHttpContextAccessor usage in services for better testability and readability.
public interface ICurrentUserService
{
    string? Role { get; }
    int? UserId { get; }
    int? CustomerId { get; }
    int? StoreId { get; }
    int? StaffId { get; }

    bool IsAdmin { get; }
    bool IsStaff { get; }
    bool IsCustomer { get; }
}
