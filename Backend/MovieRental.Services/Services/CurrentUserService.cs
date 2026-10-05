using Microsoft.AspNetCore.Http;
using MovieRental.Services.Interfaces;
using System.Security.Claims;

namespace MovieRental.Services.Services;

// Reads the current authenticated user's identity from the active HTTP request's JWT claims.
// Centralizes all claim parsing so services never touch IHttpContextAccessor directly.
public class CurrentUserService : ICurrentUserService
{
    private readonly ClaimsPrincipal? _user;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _user = httpContextAccessor.HttpContext?.User;
    }

    // Role claim: "Admin", "Staff", "Customer", or null if unauthenticated.
    public string? Role
        => _user?.FindFirst(ClaimTypes.Role)?.Value;

    // UserId from NameIdentifier claim set during token generation.
    public int? UserId
        => int.TryParse(_user?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

    // CustomerId claim — only present for users with a Customer profile.
    public int? CustomerId
        => int.TryParse(_user?.FindFirst("customerId")?.Value, out var id) ? id : null;

    // StoreId claim — only present for Staff users assigned to a store.
    public int? StoreId
        => int.TryParse(_user?.FindFirst("storeId")?.Value, out var id) ? id : null;

    // StaffId claim — only present for Staff users.
    public int? StaffId
        => int.TryParse(_user?.FindFirst("staffId")?.Value, out var id) ? id : null;

    public bool IsAdmin    => Role == "Admin";
    public bool IsStaff    => Role == "Staff";
    public bool IsCustomer => Role == "Customer";
}
