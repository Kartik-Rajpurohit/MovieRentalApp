using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Auth;
using MovieRental.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace MovieRental.Apis.Controllers;

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
            Response.Cookies.Delete("refreshToken");
            return Unauthorized(ex.Message);
        }
    }

    // Logout — reads token from HttpOnly cookie, clears it, requires valid JWT
    // Uses "AuthenticatedOnly" so Unassigned users can also log out
    [HttpPost("logout")]
    [Authorize(Policy = "AuthenticatedOnly")]
    public async Task<IActionResult> Logout()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var cookieToken = Request.Cookies["refreshToken"];
        if (!string.IsNullOrEmpty(cookieToken))
        {
            try
            {
                await _authService.LogoutAsync(cookieToken, userId);
            }
            catch (UnauthorizedAccessException)
            {
                // Token mismatch — still clear the cookie
            }
        }

        Response.Cookies.Delete("refreshToken");
        return Ok();
    }

    // Sets the refresh token as an HttpOnly cookie scoped to auth endpoints only
    private void SetRefreshTokenCookie(string refreshToken)
    {
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly  = true,               // JavaScript cannot access this cookie
            Secure    = true,               // HTTPS only
            SameSite  = SameSiteMode.Lax,  // Works for localhost cross-port (dev) + same-site prod
            Expires   = DateTimeOffset.UtcNow.AddDays(7),
            Path      = "/api/Auth"         // Cookie only sent to /api/Auth/* — minimal exposure
        });
    }
}