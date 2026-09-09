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

// Handles authentication business logic: credential verification, token generation, and session revocation.
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    // Receives repository for user queries, configuration for JWT keys, and logger for security audits.
    public AuthService(IUserRepository userRepository, IConfiguration config, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _config = config;
        _logger = logger;
    }

    // Verifies user credentials, checks account status, and issues JWT access and refresh tokens.
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        // Find user by email in the repository.
        var user = await _userRepository.GetUserByEmailAsync(dto.Email);
        if (user == null)
        {
            _logger.LogWarning("Login failed: account not found for email {Email}", dto.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        bool isPasswordValid = false;

        try
        {
            // Verify provided password against stored BCrypt hash.
            if (BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                isPasswordValid = true;
            }
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Fallback for legacy plaintext password format if applicable.
            isPasswordValid = false;
        }

        // Seamless auto-upgrade: If legacy plaintext matches, hash with BCrypt and save.
        if (!isPasswordValid && user.PasswordHash == dto.Password)
        {
            isPasswordValid = true;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            await _userRepository.UpdateUserAsync(user);
        }

        // Reject login if password does not match.
        if (!isPasswordValid)
        {
            _logger.LogWarning("Login failed: invalid password attempt for email {Email}", dto.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Prevent login if the user account is deactivated.
        if (!user.IsActive)
        {
            _logger.LogWarning("Login rejected: user account {Email} is inactive", dto.Email);
            throw new UnauthorizedAccessException("User account is inactive");
        }

        // Generate JWT access token and secure refresh token.
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // Store the refresh token in the database with 7 days validity.
        await _userRepository.SaveRefreshTokenAsync(
            user.UserId, refreshToken, DateTime.UtcNow.AddDays(7));

        _logger.LogInformation("User logged in successfully: {Email} (UserId: {UserId}, Role: {Role})",
            user.Email, user.UserId, user.Role?.RoleName ?? "Unassigned");

        // Map user details and tokens to response DTO.
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

    // Registers a new user account with hashed password and generates initial session tokens.
    public async Task<AuthResponseDto> SignUpAsync(SignUpDto dto)
    {
        // Ensure email is unique across all user accounts.
        if (await _userRepository.EmailExistsAsync(dto.Email))
        {
            _logger.LogWarning("Signup rejected: email {Email} is already registered", dto.Email);
            throw new InvalidOperationException("Email already registered");
        }

        // Determine AddressId — use existing address if chosen or create a new address record.
        int? addressId = null;

        if (dto.ExistingAddressId.HasValue)
        {
            addressId = dto.ExistingAddressId.Value;
        }
        else if (!string.IsNullOrWhiteSpace(dto.Street) && dto.CityId.HasValue)
        {
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

        // Hash the plain text password before saving to the database.
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

        // Create the user record through repository.
        var created = await _userRepository.CreateUserAsync(user);

        // Reload user to include related navigation properties for token claims.
        var reloaded = await _userRepository.GetUserByIdAsync(created.UserId);
        if (reloaded == null)
            throw new InvalidOperationException("Failed to create user");

        // Issue access and refresh tokens for immediate login after signup.
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

    // Validates an active refresh token, rotates it, and issues a new access token.
    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        // Find user by their stored refresh token.
        var user = await _userRepository.GetUserByRefreshTokenAsync(dto.RefreshToken);

        if (user == null)
        {
            _logger.LogWarning("Token refresh failed: invalid refresh token");
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Check if the refresh token has expired.
        if (user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            _logger.LogWarning("Token refresh failed: refresh token expired for user {Email}", user.Email);
            throw new UnauthorizedAccessException("Refresh token expired, please login again");
        }

        // Generate a new access token and rotate the refresh token.
        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        // Replace the old refresh token with the new one in the database.
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

    // Generates a short-lived (15-minute) signed JWT token with user identity and role claims.
    private string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Build standard claims: identity, email, name, and role.
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Unassigned")
        };

        // Add customer claim if the user has an associated customer profile.
        if (user.Customer != null)
        {
            claims.Add(new Claim("customerId", user.Customer.CustomerId.ToString()));
        }

        // Add staff and store claims if the user is a staff member.
        if (user.Staff != null)
        {
            claims.Add(new Claim("staffId", user.Staff.StaffId.ToString()));
            claims.Add(new Claim("storeId", user.Staff.StoreId.ToString()));
        }

        // Create token descriptor with 15 minutes expiration.
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            SigningCredentials = creds
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(descriptor);
    }

    // Generates a cryptographically secure 64-byte random string for refresh tokens.
    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    // Revokes the given refresh token from the database during user logout.
    public async Task LogoutAsync(string refreshToken, int? userId = null)
    {
        var user = await _userRepository.GetUserByRefreshTokenAsync(refreshToken);
        if (user == null)
        {
            _logger.LogInformation("Logout: refresh token not found in database or already revoked.");
            return;
        }

        // Ensure token belongs to the requesting user if user ID is specified.
        if (userId.HasValue && user.UserId != userId.Value)
        {
            _logger.LogWarning("Logout rejected: token mismatch for UserId {UserId} vs Owner {OwnerId}", userId.Value, user.UserId);
            return;
        }

        // Revoke token in repository.
        await _userRepository.RevokeRefreshTokenAsync(refreshToken, user.UserId);
        _logger.LogInformation("User {UserId} logged out and refresh token revoked", user.UserId);
    }

    // Revokes all active refresh tokens for the given user ID to terminate all sessions.
    public async Task LogoutByUserIdAsync(int userId)
    {
        await _userRepository.RevokeRefreshTokenByUserIdAsync(userId);
        _logger.LogInformation("User {UserId} logged out by UserId and refresh token revoked", userId);
    }
}