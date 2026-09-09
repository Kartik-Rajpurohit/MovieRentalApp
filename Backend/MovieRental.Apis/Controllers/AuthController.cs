using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Auth;
using MovieRental.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace MovieRental.Apis.Controllers;

// Handles user authentication, registration, token refresh, and logout.
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("AuthRateLimit")] // Max 10 requests/min per IP on all auth endpoints
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // Authenticates user and sets HttpOnly refresh token cookie.
    [HttpPost("login")]
    [AllowAnonymous] // Public — no token needed
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var result = await _authService.LoginAsync(dto);
            SetRefreshTokenCookie(result.RefreshToken);
            return Ok(result); // RefreshToken is [JsonIgnore] — not included in response body
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    // Registers a new user account.
    [HttpPost("signup")]
    [AllowAnonymous] // Public — no token needed
    public async Task<IActionResult> SignUp([FromBody] SignUpDto dto)
    {
        try
        {
            var result = await _authService.SignUpAsync(dto);
            SetRefreshTokenCookie(result.RefreshToken);
            return Ok(result); // RefreshToken is [JsonIgnore] — not included in response body
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    // Refresh — reads token from HttpOnly cookie, no body required
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh()
    {
        var cookieToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(cookieToken))
            return Unauthorized("No refresh token found");

        try
        {
            var result = await _authService.RefreshTokenAsync(new RefreshTokenDto { RefreshToken = cookieToken });
            SetRefreshTokenCookie(result.RefreshToken);
            return Ok(result); // RefreshToken is [JsonIgnore] — not included in response body
        }
        catch (UnauthorizedAccessException ex)
        {
            ClearRefreshTokenCookie();
            return Unauthorized(ex.Message);
        }
    }

    // Logout — reads token from HttpOnly cookie (or JWT claim), clears it, revokes refresh token
    // Uses [AllowAnonymous] to bypass the global RequireRole policy so Unassigned users can also log out
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        var cookieToken = Request.Cookies["refreshToken"];
        int? userId = null;
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value
                     ?? User.FindFirst("nameid")?.Value;
        if (int.TryParse(userIdStr, out var id))
        {
            userId = id;
        }

        try
        {
            if (!string.IsNullOrEmpty(cookieToken))
            {
                await _authService.LogoutAsync(cookieToken, userId);
            }
            else if (userId.HasValue)
            {
                // Fallback: If the browser omitted the cookie (e.g. cross-origin/cross-port restriction in dev),
                // revoke the refresh token directly using the authenticated UserId from the JWT bearer token
                await _authService.LogoutByUserIdAsync(userId.Value);
            }
        }
        catch
        {
            // Silently proceed so cookie is always cleared
        }

        ClearRefreshTokenCookie();
        return Ok();
    }

    // Sets the refresh token as an HttpOnly cookie scoped to auth endpoints only
    private void SetRefreshTokenCookie(string refreshToken)
    {
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly  = true,               // JavaScript cannot access this cookie
            Secure    = true,               // HTTPS only
            SameSite  = SameSiteMode.None,  // None allows cross-origin requests from frontend to backend
            Expires   = DateTimeOffset.UtcNow.AddDays(7),
            Path      = "/api/Auth"         // Cookie only sent to /api/Auth/* — minimal exposure
        });
    }

    // Clears the refresh token cookie with matching path options so browser deletes it
    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly  = true,
            Secure    = true,
            SameSite  = SameSiteMode.None,
            Path      = "/api/Auth"
        });
    }
}