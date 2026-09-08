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

using Microsoft.Extensions.Logging;

namespace MovieRental.Services.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, IConfiguration config, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _config = config;
        _logger = logger;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetUserByEmailAsync(dto.Email);
        if (user == null)
        {
            _logger.LogWarning("Login failed: account not found for email {Email}", dto.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        bool isPasswordValid = false;

        try
        {
            if (BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                isPasswordValid = true;
            }
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Fallback for legacy plaintext password format
            isPasswordValid = false;
        }

        // Seamless auto-upgrade: If legacy password matches, upgrade to BCrypt immediately
        if (!isPasswordValid && user.PasswordHash == dto.Password)
        {
            isPasswordValid = true;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            await _userRepository.UpdateUserAsync(user);
        }

        if (!isPasswordValid)
        {
            _logger.LogWarning("Login failed: invalid password attempt for email {Email}", dto.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login rejected: user account {Email} is inactive", dto.Email);
            throw new UnauthorizedAccessException("User account is inactive");
        }

        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // Refresh token 7 din ke liye valid — DB mein save
        await _userRepository.SaveRefreshTokenAsync(
            user.UserId, refreshToken, DateTime.UtcNow.AddDays(7));

        _logger.LogInformation("User logged in successfully: {Email} (UserId: {UserId}, Role: {Role})",
            user.Email, user.UserId, user.Role?.RoleName ?? "Unassigned");

        return new AuthResponseDto
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            Role = user.Role?.RoleName ?? "Unassigned",
            CustomerId = user.Customer?.CustomerId,
            StoreId = user.Staff?.StoreId,
            StaffId = user.Staff?.StaffId,
            Token = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResponseDto> SignUpAsync(SignUpDto dto)
    {
        if (await _userRepository.EmailExistsAsync(dto.Email))
        {
            _logger.LogWarning("Signup rejected: email {Email} is already registered", dto.Email);
            throw new InvalidOperationException("Email already registered");
        }

        // Determine AddressId — use existing or create new
        int? addressId = null;

        if (dto.ExistingAddressId.HasValue)
        {
            // User selected an existing address from suggestions
            addressId = dto.ExistingAddressId.Value;
        }
        else if (!string.IsNullOrWhiteSpace(dto.Street) && dto.CityId.HasValue)
        {
            // User typed a new address — create it in DB
            var newAddress = new Address
            {
                Street = dto.Street,
                PostalCode = dto.PostalCode,
                Phone = dto.Phone ?? string.Empty,
                CityId = dto.CityId.Value,
                LastUpdate = DateTime.UtcNow
            };
            addressId = await _userRepository.CreateAddressAsync(newAddress);
        }

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = null,
            AddressId = addressId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _userRepository.CreateUserAsync(user);

        var reloaded = await _userRepository.GetUserByIdAsync(created.UserId);
        if (reloaded == null)
            throw new InvalidOperationException("Failed to create user");

        var accessToken = GenerateAccessToken(reloaded);
        var refreshToken = GenerateRefreshToken();

        await _userRepository.SaveRefreshTokenAsync(
            reloaded.UserId, refreshToken, DateTime.UtcNow.AddDays(7));

        _logger.LogInformation("New user signed up successfully: {Email} (UserId: {UserId})",
            reloaded.Email, reloaded.UserId);

        return new AuthResponseDto
        {
            UserId = reloaded.UserId,
            Email = reloaded.Email,
            FullName = $"{reloaded.FirstName} {reloaded.LastName}".Trim(),
            Role = reloaded.Role?.RoleName ?? "Unassigned",
            CustomerId = reloaded.Customer?.CustomerId,
            StoreId = reloaded.Staff?.StoreId,
            StaffId = reloaded.Staff?.StaffId,
            Token = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        // DB se user dhundo refresh token se
        var user = await _userRepository.GetUserByRefreshTokenAsync(dto.RefreshToken);

        if (user == null)
        {
            _logger.LogWarning("Token refresh failed: invalid refresh token");
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Expiry check
        if (user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            _logger.LogWarning("Token refresh failed: refresh token expired for user {Email}", user.Email);
            throw new UnauthorizedAccessException("Refresh token expired, please login again");
        }

        // Naya access token + naya refresh token generate karo (rotation)
        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        // Purana refresh token replace karo
        await _userRepository.SaveRefreshTokenAsync(
            user.UserId, newRefreshToken, DateTime.UtcNow.AddDays(7));

        _logger.LogInformation("Token refreshed successfully for user {Email} (UserId: {UserId})",
            user.Email, user.UserId);

        return new AuthResponseDto
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            Role = user.Role?.RoleName ?? "Unassigned",
            CustomerId = user.Customer?.CustomerId,
            StoreId = user.Staff?.StoreId,
            StaffId = user.Staff?.StaffId,
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

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Unassigned")
        };

        if (user.Customer != null)
        {
            claims.Add(new Claim("customerId", user.Customer.CustomerId.ToString()));
        }

        if (user.Staff != null)
        {
            claims.Add(new Claim("staffId", user.Staff.StaffId.ToString()));
            claims.Add(new Claim("storeId", user.Staff.StoreId.ToString()));
        }

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

    public async Task LogoutAsync(string refreshToken, int userId)
    {
        // Verify the token actually belongs to this user before revoking
        var user = await _userRepository.GetUserByRefreshTokenAsync(refreshToken);
        if (user == null || user.UserId != userId)
        {
            _logger.LogWarning("Logout rejected: token mismatch or not found for UserId {UserId}", userId);
            throw new UnauthorizedAccessException("Invalid or mismatched refresh token");
        }

        await _userRepository.RevokeRefreshTokenAsync(refreshToken, userId);
        _logger.LogInformation("User {UserId} logged out and refresh token revoked", userId);
    }
}