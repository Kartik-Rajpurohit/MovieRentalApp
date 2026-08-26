using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MovieRental.Domain.DTOs.Auth;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository userRepository, IConfiguration config)
    {
        _userRepository = userRepository;
        _config = config;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetUserByEmailAsync(dto.Email);
        if (user == null)
            throw new UnauthorizedAccessException("No User Found");

        if (user.PasswordHash != dto.Password)
            throw new UnauthorizedAccessException("Invalid email or password");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("User account is inactive");

        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // Refresh token 7 din ke liye valid — DB mein save
        await _userRepository.SaveRefreshTokenAsync(
            user.UserId, refreshToken, DateTime.UtcNow.AddDays(7));

        return new AuthResponseDto
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            Role = user.Role?.RoleName ?? "Unassigned",
            Token = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResponseDto> SignUpAsync(SignUpDto dto)
    {
        if (await _userRepository.EmailExistsAsync(dto.Email))
            throw new InvalidOperationException("Email already registered");

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = dto.Password,
            RoleId = null,
            AddressId = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _userRepository.CreateUserAsync(user);
        await _userRepository.CreateCustomerAsync(created.UserId, 1);

        var reloaded = await _userRepository.GetUserByIdAsync(created.UserId);
        if (reloaded == null)
            throw new InvalidOperationException("Failed to create user");

        var accessToken = GenerateAccessToken(reloaded);
        var refreshToken = GenerateRefreshToken();

        await _userRepository.SaveRefreshTokenAsync(
            reloaded.UserId, refreshToken, DateTime.UtcNow.AddDays(7));

        return new AuthResponseDto
        {
            UserId = reloaded.UserId,
            Email = reloaded.Email,
            FullName = $"{reloaded.FirstName} {reloaded.LastName}".Trim(),
            Role = reloaded.Role?.RoleName ?? "Unassigned",
            Token = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        // DB se user dhundo refresh token se
        var user = await _userRepository.GetUserByRefreshTokenAsync(dto.RefreshToken);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        // Expiry check
        if (user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expired, please login again");

        // Naya access token + naya refresh token generate karo (rotation)
        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        // Purana refresh token replace karo
        await _userRepository.SaveRefreshTokenAsync(
            user.UserId, newRefreshToken, DateTime.UtcNow.AddDays(7));

        return new AuthResponseDto
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            Role = user.Role?.RoleName ?? "Unassigned",
            Token = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }

    // Access token — 15 minute valid (short lived)
    private string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Unassigned")
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),  // 15 min
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            SigningCredentials = creds
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(descriptor);
    }

    // Refresh token — random 64 byte string, DB mein store hota hai
    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        await _userRepository.RevokeRefreshTokenAsync(refreshToken);
    }
}