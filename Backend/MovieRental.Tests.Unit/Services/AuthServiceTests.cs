using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MovieRental.Domain.DTOs.Auth;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;
using MovieRental.Services.Services;
using Xunit;

namespace MovieRental.Tests.Unit.Services;

public class AuthServiceTests
{
    // Precomputing BCrypt hash with workFactor: 4 cuts test execution time by ~16x while preserving verification logic
    private static readonly string ValidPassword = "Password123!";
    private static readonly string PrecomputedHash = BCrypt.Net.BCrypt.HashPassword(ValidPassword, workFactor: 4);

    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICountryRepository> _countryRepositoryMock;
    private readonly Mock<ICityRepository> _cityRepositoryMock;
    private readonly Mock<IGeoapifyService> _geoapifyServiceMock;
    private readonly IConfiguration _config;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        // 1. Create Moq mocks
        _userRepositoryMock = new Mock<IUserRepository>();
        _countryRepositoryMock = new Mock<ICountryRepository>();
        _cityRepositoryMock = new Mock<ICityRepository>();
        _geoapifyServiceMock = new Mock<IGeoapifyService>();

        // 2. Set up valid in-memory JWT configuration
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:SecretKey", "test_jwt_secret_key_at_least_32_characters_long_12345" },
            { "Jwt:Issuer", "MovieRentalApp" },
            { "Jwt:Audience", "MovieRentalAppUsers" }
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // 3. Inject mock.Object into the service
        _sut = new AuthService(
            _userRepositoryMock.Object,
            _countryRepositoryMock.Object,
            _cityRepositoryMock.Object,
            _geoapifyServiceMock.Object,
            _config,
            NullLogger<AuthService>.Instance
        );
    }

    // =========================================================
    // LoginAsync Tests
    // =========================================================

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync("unknown@example.com"))
            .ReturnsAsync((User?)null);

        var dto = new LoginDto { Email = "unknown@example.com", Password = "Password123!" };

        // Act
        Func<Task> act = async () => await _sut.LoginAsync(dto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIncorrect_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Email = "john@example.com",
            PasswordHash = PrecomputedHash,
            IsActive = true
        };
        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(user.Email))
            .ReturnsAsync(user);

        var dto = new LoginDto { Email = user.Email, Password = "WrongPassword!" };

        // Act
        Func<Task> act = async () => await _sut.LoginAsync(dto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WhenUserIsInactive_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Email = "inactive@example.com",
            PasswordHash = PrecomputedHash,
            IsActive = false
        };
        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(user.Email))
            .ReturnsAsync(user);

        var dto = new LoginDto { Email = user.Email, Password = ValidPassword };

        // Act
        Func<Task> act = async () => await _sut.LoginAsync(dto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User account is inactive");
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsValidAndActive_ReturnsAuthResponseAndSavesRefreshToken()
    {
        // Arrange
        var user = new User
        {
            UserId = 10,
            Email = "alice@example.com",
            FirstName = "Alice",
            LastName = "Smith",
            PasswordHash = PrecomputedHash,
            IsActive = true,
            Role = new Role { RoleName = "Admin" }
        };
        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(user.Email))
            .ReturnsAsync(user);

        var dto = new LoginDto { Email = user.Email, Password = ValidPassword };

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(10);
        result.Email.Should().Be("alice@example.com");
        result.FullName.Should().Be("Alice Smith");
        result.Role.Should().Be("Admin");
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();

        // Verify side effect with Moq Verify
        _userRepositoryMock.Verify(r => r.SaveRefreshTokenAsync(
            user.UserId,
            It.IsAny<string>(),
            It.IsAny<DateTime>()
        ), Times.Once);
    }

    [Fact]
    public async Task Login_WhenSuccessful_SetsRefreshTokenRevokedFalse()
    {
        // Arrange
        var user = new User
        {
            UserId = 11,
            Email = "john.revoked@example.com",
            PasswordHash = PrecomputedHash,
            IsActive = true,
            RefreshToken = "old-revoked-token",
            RefreshTokenRevoked = true,
            RefreshTokenRevokedAt = DateTime.UtcNow.AddHours(-1)
        };
        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(user.Email))
            .ReturnsAsync(user);

        _userRepositoryMock.Setup(r => r.SaveRefreshTokenAsync(user.UserId, It.IsAny<string>(), It.IsAny<DateTime>()))
            .Callback<int, string, DateTime>((id, token, exp) =>
            {
                user.RefreshToken = token;
                user.RefreshTokenExpiry = exp;
                user.RefreshTokenRevoked = false;
                user.RefreshTokenRevokedAt = null;
            })
            .Returns(Task.CompletedTask);

        var dto = new LoginDto { Email = user.Email, Password = ValidPassword };

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Should().NotBeNull();
        user.RefreshTokenRevoked.Should().BeFalse();
        user.RefreshTokenRevokedAt.Should().BeNull();
        _userRepositoryMock.Verify(r => r.SaveRefreshTokenAsync(user.UserId, It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task Login_AfterPreviousLogout_CreatesNewRefreshTokenAndUnrevokesNewSession()
    {
        // Arrange: User was previously logged out with revoked Token A
        var oldToken = "token-a-hash";
        var user = new User
        {
            UserId = 12,
            Email = "loggedout@example.com",
            PasswordHash = PrecomputedHash,
            IsActive = true,
            RefreshToken = oldToken,
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(5),
            RefreshTokenRevoked = true,
            RefreshTokenRevokedAt = DateTime.UtcNow.AddDays(-1)
        };
        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(user.Email))
            .ReturnsAsync(user);

        _userRepositoryMock.Setup(r => r.SaveRefreshTokenAsync(user.UserId, It.IsAny<string>(), It.IsAny<DateTime>()))
            .Callback<int, string, DateTime>((id, token, exp) =>
            {
                user.RefreshToken = token;
                user.RefreshTokenExpiry = exp;
                user.RefreshTokenRevoked = false;
                user.RefreshTokenRevokedAt = null;
            })
            .Returns(Task.CompletedTask);

        var dto = new LoginDto { Email = user.Email, Password = ValidPassword };

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert: New token created, unrevoked, distinct from old token
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBe(oldToken);
        user.RefreshTokenRevoked.Should().BeFalse();
        user.RefreshTokenRevokedAt.Should().BeNull();
    }

    // =========================================================
    // RefreshTokenAsync Tests
    // =========================================================

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenNotFound_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetUserByRefreshTokenAsync("invalid-token"))
            .ReturnsAsync((User?)null);

        var dto = new RefreshTokenDto { RefreshToken = "invalid-token" };

        // Act
        Func<Task> act = async () => await _sut.RefreshTokenAsync(dto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid refresh token");
    }

    [Fact]
    public async Task RefreshToken_WhenRevoked_ReturnsUnauthorized()
    {
        // Arrange
        var user = new User
        {
            UserId = 13,
            Email = "revoked@example.com",
            RefreshToken = "revoked-token",
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(2),
            RefreshTokenRevoked = true,
            RefreshTokenRevokedAt = DateTime.UtcNow.AddMinutes(-10)
        };
        _userRepositoryMock.Setup(r => r.GetUserByRefreshTokenAsync("revoked-token"))
            .ReturnsAsync(user);

        var dto = new RefreshTokenDto { RefreshToken = "revoked-token" };

        // Act
        Func<Task> act = async () => await _sut.RefreshTokenAsync(dto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token has been revoked");

        // Verify no new token saved
        _userRepositoryMock.Verify(r => r.SaveRefreshTokenAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task RefreshToken_WhenNotRevokedAndNotExpired_Succeeds()
    {
        // Arrange
        var user = new User
        {
            UserId = 14,
            Email = "valid@example.com",
            RefreshToken = "valid-token",
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(3),
            RefreshTokenRevoked = false,
            RefreshTokenRevokedAt = null,
            Role = new Role { RoleName = "Customer" }
        };
        _userRepositoryMock.Setup(r => r.GetUserByRefreshTokenAsync("valid-token"))
            .ReturnsAsync(user);

        var dto = new RefreshTokenDto { RefreshToken = "valid-token" };

        // Act
        var result = await _sut.RefreshTokenAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBe("valid-token");
        _userRepositoryMock.Verify(r => r.SaveRefreshTokenAsync(user.UserId, result.RefreshToken, It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task RefreshToken_WhenExpired_ReturnsUnauthorized()
    {
        // Arrange: Token expired 5 minutes ago
        var user = new User
        {
            UserId = 15,
            Email = "expired@example.com",
            RefreshToken = "expired-token",
            RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(-5),
            RefreshTokenRevoked = false,
            RefreshTokenRevokedAt = null
        };
        _userRepositoryMock.Setup(r => r.GetUserByRefreshTokenAsync("expired-token"))
            .ReturnsAsync(user);

        var dto = new RefreshTokenDto { RefreshToken = "expired-token" };

        // Act
        Func<Task> act = async () => await _sut.RefreshTokenAsync(dto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token expired, please login again");
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenExpired_ThrowsUnauthorizedAccessException()
    {
        // Arrange: Token expired 5 minutes ago
        var user = new User
        {
            UserId = 1,
            Email = "user@example.com",
            RefreshToken = "expired-token",
            RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(-5),
            RefreshTokenRevoked = false
        };
        _userRepositoryMock.Setup(r => r.GetUserByRefreshTokenAsync("expired-token"))
            .ReturnsAsync(user);

        var dto = new RefreshTokenDto { RefreshToken = "expired-token" };

        // Act
        Func<Task> act = async () => await _sut.RefreshTokenAsync(dto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token expired, please login again");
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenValid_RotatesTokenAndReturnsAuthResponse()
    {
        // Arrange: Valid token expiring in 2 days
        var oldRefreshToken = "valid-old-refresh-token";
        var user = new User
        {
            UserId = 2,
            Email = "bob@example.com",
            FirstName = "Bob",
            LastName = "Jones",
            RefreshToken = oldRefreshToken,
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(2),
            RefreshTokenRevoked = false,
            Role = new Role { RoleName = "Staff" }
        };
        _userRepositoryMock.Setup(r => r.GetUserByRefreshTokenAsync(oldRefreshToken))
            .ReturnsAsync(user);

        var dto = new RefreshTokenDto { RefreshToken = oldRefreshToken };
        var beforeCall = DateTime.UtcNow;

        // Act
        var result = await _sut.RefreshTokenAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("bob@example.com");
        result.Role.Should().Be("Staff");
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBe(oldRefreshToken);

        // Assert robust time window (> 6 days from call time)
        _userRepositoryMock.Verify(r => r.SaveRefreshTokenAsync(
            user.UserId,
            result.RefreshToken,
            It.Is<DateTime>(dt => dt >= beforeCall.AddDays(6))
        ), Times.Once);
    }

    // =========================================================
    // LogoutAsync Tests
    // =========================================================

    [Theory]
    [InlineData(null)]
    [InlineData(42)]
    public async Task LogoutAsync_WhenTokenNotFound_ExitsGracefullyWithoutRevoking(int? userId)
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetUserByRefreshTokenAsync("non-existent-token"))
            .ReturnsAsync((User?)null);

        // Act
        await _sut.LogoutAsync("non-existent-token", userId);

        // Assert
        _userRepositoryMock.Verify(r => r.RevokeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_WhenUserIdMismatchesTokenOwner_DoesNotRevokeToken()
    {
        // Arrange: Token belongs to user #10
        var user = new User { UserId = 10 };
        _userRepositoryMock.Setup(r => r.GetUserByRefreshTokenAsync("token-of-user-10"))
            .ReturnsAsync(user);

        // Act: User #99 attempts to log out with user #10's token
        await _sut.LogoutAsync("token-of-user-10", userId: 99);

        // Assert
        _userRepositoryMock.Verify(r => r.RevokeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_WhenTokenAndUserIdMatch_RevokesRefreshToken()
    {
        // Arrange: Token belongs to user #5
        var token = "user5-token";
        var user = new User { UserId = 5 };
        _userRepositoryMock.Setup(r => r.GetUserByRefreshTokenAsync(token))
            .ReturnsAsync(user);

        // Act: User #5 logs out
        await _sut.LogoutAsync(token, userId: 5);

        // Assert
        _userRepositoryMock.Verify(r => r.RevokeRefreshTokenAsync(token, 5), Times.Once);
    }

    [Fact]
    public async Task Logout_MarksRefreshTokenAsRevoked()
    {
        // Arrange
        var token = "active-token-logout";
        var user = new User
        {
            UserId = 16,
            RefreshToken = token,
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
            RefreshTokenRevoked = false,
            RefreshTokenRevokedAt = null
        };
        _userRepositoryMock.Setup(r => r.GetUserByRefreshTokenAsync(token))
            .ReturnsAsync(user);

        _userRepositoryMock.Setup(r => r.RevokeRefreshTokenAsync(token, user.UserId))
            .Callback<string, int?>((t, uid) =>
            {
                user.RefreshTokenRevoked = true;
                user.RefreshTokenRevokedAt = DateTime.UtcNow;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _sut.LogoutAsync(token, user.UserId);

        // Assert
        user.RefreshTokenRevoked.Should().BeTrue();
        _userRepositoryMock.Verify(r => r.RevokeRefreshTokenAsync(token, user.UserId), Times.Once);
    }

    [Fact]
    public async Task Logout_SetsRefreshTokenRevokedAt()
    {
        // Arrange
        var token = "active-token-revoked-at";
        var user = new User
        {
            UserId = 17,
            RefreshToken = token,
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
            RefreshTokenRevoked = false,
            RefreshTokenRevokedAt = null
        };
        _userRepositoryMock.Setup(r => r.GetUserByRefreshTokenAsync(token))
            .ReturnsAsync(user);

        var beforeCall = DateTime.UtcNow;
        _userRepositoryMock.Setup(r => r.RevokeRefreshTokenAsync(token, user.UserId))
            .Callback<string, int?>((t, uid) =>
            {
                user.RefreshTokenRevoked = true;
                user.RefreshTokenRevokedAt = DateTime.UtcNow;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _sut.LogoutAsync(token, user.UserId);

        // Assert
        user.RefreshTokenRevokedAt.Should().NotBeNull();
        user.RefreshTokenRevokedAt.Should().BeOnOrAfter(beforeCall);
    }

    [Fact]
    public async Task Logout_DoesNotClearRefreshToken()
    {
        // Arrange
        var token = "active-token-preserve-hash";
        var user = new User
        {
            UserId = 18,
            RefreshToken = token,
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
            RefreshTokenRevoked = false,
            RefreshTokenRevokedAt = null
        };
        _userRepositoryMock.Setup(r => r.GetUserByRefreshTokenAsync(token))
            .ReturnsAsync(user);

        _userRepositoryMock.Setup(r => r.RevokeRefreshTokenAsync(token, user.UserId))
            .Callback<string, int?>((t, uid) =>
            {
                user.RefreshTokenRevoked = true;
                user.RefreshTokenRevokedAt = DateTime.UtcNow;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _sut.LogoutAsync(token, user.UserId);

        // Assert: Token value is NOT cleared
        user.RefreshToken.Should().Be(token);
    }

    [Fact]
    public async Task Logout_DoesNotClearRefreshTokenExpiry()
    {
        // Arrange
        var token = "active-token-preserve-expiry";
        var expiry = DateTime.UtcNow.AddDays(7);
        var user = new User
        {
            UserId = 19,
            RefreshToken = token,
            RefreshTokenExpiry = expiry,
            RefreshTokenRevoked = false,
            RefreshTokenRevokedAt = null
        };
        _userRepositoryMock.Setup(r => r.GetUserByRefreshTokenAsync(token))
            .ReturnsAsync(user);

        _userRepositoryMock.Setup(r => r.RevokeRefreshTokenAsync(token, user.UserId))
            .Callback<string, int?>((t, uid) =>
            {
                user.RefreshTokenRevoked = true;
                user.RefreshTokenRevokedAt = DateTime.UtcNow;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _sut.LogoutAsync(token, user.UserId);

        // Assert: Expiry is NOT cleared
        user.RefreshTokenExpiry.Should().Be(expiry);
    }

    // =========================================================
    // Re-login after logout Lifecycle Test
    // =========================================================

    [Fact]
    public async Task ReLoginAfterLogout_FullLifecycle_MaintainsRevocationStateAccurately()
    {
        // Setup initial user state
        var user = new User
        {
            UserId = 42,
            Email = "lifecycle@example.com",
            PasswordHash = PrecomputedHash,
            IsActive = true,
            Role = new Role { RoleName = "Staff" }
        };

        // Simulated repository state
        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(user.Email))
            .ReturnsAsync(user);

        _userRepositoryMock.Setup(r => r.SaveRefreshTokenAsync(user.UserId, It.IsAny<string>(), It.IsAny<DateTime>()))
            .Callback<int, string, DateTime>((uid, tok, exp) =>
            {
                user.RefreshToken = tok;
                user.RefreshTokenExpiry = exp;
                user.RefreshTokenRevoked = false;
                user.RefreshTokenRevokedAt = null;
            })
            .Returns(Task.CompletedTask);

        _userRepositoryMock.Setup(r => r.RevokeRefreshTokenAsync(It.IsAny<string>(), user.UserId))
            .Callback<string, int?>((tok, uid) =>
            {
                user.RefreshTokenRevoked = true;
                user.RefreshTokenRevokedAt = DateTime.UtcNow;
            })
            .Returns(Task.CompletedTask);

        _userRepositoryMock.Setup(r => r.GetUserByRefreshTokenAsync(It.IsAny<string>()))
            .Returns<string>(tok => Task.FromResult<User?>(user.RefreshToken == tok ? user : null));

        // 1. LOGIN
        var loginDto = new LoginDto { Email = user.Email, Password = ValidPassword };
        var loginResult1 = await _sut.LoginAsync(loginDto);
        var tokenA = loginResult1.RefreshToken;

        user.RefreshToken.Should().Be(tokenA);
        user.RefreshTokenRevoked.Should().BeFalse();
        user.RefreshTokenRevokedAt.Should().BeNull();

        // 2. LOGOUT
        await _sut.LogoutAsync(tokenA, user.UserId);

        user.RefreshToken.Should().Be(tokenA); // Token A preserved
        user.RefreshTokenRevoked.Should().BeTrue(); // Revoked
        user.RefreshTokenRevokedAt.Should().NotBeNull();

        // 3. REFRESH USING TOKEN A (Must fail 401 with revocation message)
        var refreshDtoA = new RefreshTokenDto { RefreshToken = tokenA };
        Func<Task> actRefreshA = async () => await _sut.RefreshTokenAsync(refreshDtoA);
        await actRefreshA.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token has been revoked");

        // 4. LOGIN AGAIN
        var loginResult2 = await _sut.LoginAsync(loginDto);
        var tokenB = loginResult2.RefreshToken;

        tokenB.Should().NotBe(tokenA);
        user.RefreshToken.Should().Be(tokenB);
        user.RefreshTokenRevoked.Should().BeFalse(); // Reset to false
        user.RefreshTokenRevokedAt.Should().BeNull(); // Reset to null

        // 5. REFRESH USING TOKEN B (Must succeed and rotate to Token C)
        var refreshDtoB = new RefreshTokenDto { RefreshToken = tokenB };
        var refreshResultB = await _sut.RefreshTokenAsync(refreshDtoB);

        refreshResultB.Should().NotBeNull();
        refreshResultB.Token.Should().NotBeNullOrWhiteSpace();
        refreshResultB.RefreshToken.Should().NotBeNullOrWhiteSpace();
        refreshResultB.RefreshToken.Should().NotBe(tokenB); // Rotated to Token C
        user.RefreshTokenRevoked.Should().BeFalse();
        user.RefreshTokenRevokedAt.Should().BeNull();
    }

    // =========================================================
    // SignUpAsync Tests
    // =========================================================

    [Fact]
    public async Task SignUpAsync_WhenEmailAlreadyExists_ThrowsInvalidOperationExceptionAndPerformsNoWrites()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.EmailExistsAsync("existing@example.com"))
            .ReturnsAsync(true);

        var dto = new SignUpDto
        {
            Email = "existing@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Country = "United States",
            City = "New York",
            Street = "123 Main St",
            Phone = "1234567890"
        };

        // Act
        Func<Task> act = async () => await _sut.SignUpAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email already registered");

        _countryRepositoryMock.Verify(r => r.CreateCountryAsync(It.IsAny<Country>()), Times.Never);
        _cityRepositoryMock.Verify(r => r.CreateCityAsync(It.IsAny<City>()), Times.Never);
        _userRepositoryMock.Verify(r => r.CreateAddressAsync(It.IsAny<Address>()), Times.Never);
        _userRepositoryMock.Verify(r => r.CreateUserAsync(It.IsAny<User>()), Times.Never);
        _geoapifyServiceMock.Verify(g => g.AutocompleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SignUpAsync_WhenCountryAndCityDoNotExist_CreatesLocationRecordsAndHashesPassword()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.EmailExistsAsync("newuser@example.com")).ReturnsAsync(false);

        _countryRepositoryMock.Setup(r => r.GetCountryByNameAsync("Wakanda")).ReturnsAsync((Country?)null);
        _countryRepositoryMock.Setup(r => r.CreateCountryAsync(It.IsAny<Country>()))
            .ReturnsAsync(new Country { CountryId = 101, Name = "Wakanda" });

        _cityRepositoryMock.Setup(r => r.GetCityByNameAndCountryIdAsync("Birnin Zana", 101)).ReturnsAsync((City?)null);
        _cityRepositoryMock.Setup(r => r.CreateCityAsync(It.IsAny<City>()))
            .ReturnsAsync(new City { CityId = 202, Name = "Birnin Zana", CountryId = 101 });

        _userRepositoryMock.Setup(r => r.CreateAddressAsync(It.IsAny<Address>())).ReturnsAsync(303);

        var createdUser = new User
        {
            UserId = 404,
            Email = "newuser@example.com",
            FirstName = "T'Challa",
            LastName = "King",
            IsActive = true
        };
        _userRepositoryMock.Setup(r => r.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(createdUser);
        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(404)).ReturnsAsync(createdUser);

        var dto = new SignUpDto
        {
            Email = "newuser@example.com",
            Password = "VibraniumPassword123!",
            FirstName = "T'Challa",
            LastName = "King",
            Country = "Wakanda",
            City = "Birnin Zana",
            Street = "1 Palace Way",
            Phone = "5550100123"
        };

        // Act
        var result = await _sut.SignUpAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(404);
        result.Email.Should().Be("newuser@example.com");
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();

        _countryRepositoryMock.Verify(r => r.CreateCountryAsync(It.Is<Country>(c => c.Name == "Wakanda")), Times.Once);
        _cityRepositoryMock.Verify(r => r.CreateCityAsync(It.Is<City>(c => c.Name == "Birnin Zana" && c.CountryId == 101)), Times.Once);
        _userRepositoryMock.Verify(r => r.CreateUserAsync(It.Is<User>(u =>
            u.PasswordHash != "VibraniumPassword123!" &&
            BCrypt.Net.BCrypt.Verify("VibraniumPassword123!", u.PasswordHash))), Times.Once);
    }

    [Fact]
    public async Task SignUpAsync_WhenCountryAndCityAlreadyExist_ReusesExistingRecordsWithoutCreatingNewOnes()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.EmailExistsAsync("jane@example.com")).ReturnsAsync(false);

        var existingCountry = new Country { CountryId = 50, Name = "Canada" };
        _countryRepositoryMock.Setup(r => r.GetCountryByNameAsync("Canada")).ReturnsAsync(existingCountry);

        var existingCity = new City { CityId = 60, Name = "Toronto", CountryId = 50 };
        _cityRepositoryMock.Setup(r => r.GetCityByNameAndCountryIdAsync("Toronto", 50)).ReturnsAsync(existingCity);

        _userRepositoryMock.Setup(r => r.CreateAddressAsync(It.IsAny<Address>())).ReturnsAsync(70);

        var createdUser = new User
        {
            UserId = 80,
            Email = "jane@example.com",
            FirstName = "Jane",
            LastName = "Doe",
            IsActive = true
        };
        _userRepositoryMock.Setup(r => r.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(createdUser);
        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(80)).ReturnsAsync(createdUser);

        var dto = new SignUpDto
        {
            Email = "jane@example.com",
            Password = "Password123!",
            FirstName = "Jane",
            LastName = "Doe",
            Country = "Canada",
            City = "Toronto",
            Street = "456 Queen St",
            Phone = "5550200123"
        };

        // Act
        var result = await _sut.SignUpAsync(dto);

        // Assert
        _countryRepositoryMock.Verify(r => r.CreateCountryAsync(It.IsAny<Country>()), Times.Never);
        _cityRepositoryMock.Verify(r => r.CreateCityAsync(It.IsAny<City>()), Times.Never);
        _userRepositoryMock.Verify(r => r.CreateAddressAsync(It.Is<Address>(a => a.CityId == 60)), Times.Once);
    }

    [Fact]
    public async Task SignUpAsync_WhenReloadedUserIsNull_ThrowsInvalidOperationException()
    {
        // Arrange: User creation succeeds, but reload returns null
        _userRepositoryMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _countryRepositoryMock.Setup(r => r.GetCountryByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(new Country { CountryId = 1, Name = "USA" });
        _cityRepositoryMock.Setup(r => r.GetCityByNameAndCountryIdAsync(It.IsAny<string>(), 1))
            .ReturnsAsync(new City { CityId = 1, Name = "NYC", CountryId = 1 });
        _userRepositoryMock.Setup(r => r.CreateAddressAsync(It.IsAny<Address>())).ReturnsAsync(1);
        _userRepositoryMock.Setup(r => r.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(new User { UserId = 999 });
        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(999)).ReturnsAsync((User?)null);

        var dto = new SignUpDto
        {
            Email = "ghost@example.com",
            Password = "Password123!",
            FirstName = "Ghost",
            LastName = "User",
            Country = "USA",
            City = "NYC",
            Street = "123 Wall St",
            Phone = "1234567890"
        };

        // Act
        Func<Task> act = async () => await _sut.SignUpAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to create user");
    }

    // =========================================================
    // GetAddressAutocompleteAsync Tests
    // =========================================================

    [Fact]
    public async Task GetAddressAutocompleteAsync_DelegatesToGeoapifyService()
    {
        // Arrange
        var expectedSuggestions = new List<AddressAutocompleteDto>
        {
            new() { FormattedAddress = "123 Broadway, New York, NY", City = "New York", Country = "USA" }
        };
        _geoapifyServiceMock.Setup(g => g.AutocompleteAsync("123 Broadway"))
            .ReturnsAsync(expectedSuggestions);

        // Act
        var result = await _sut.GetAddressAutocompleteAsync("123 Broadway");

        // Assert
        result.Should().BeEquivalentTo(expectedSuggestions);
        _geoapifyServiceMock.Verify(g => g.AutocompleteAsync("123 Broadway"), Times.Once);
    }
}
