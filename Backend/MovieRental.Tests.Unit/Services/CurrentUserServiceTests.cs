using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using MovieRental.Services.Services;
using Xunit;

namespace MovieRental.Tests.Unit.Services;

public class CurrentUserServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    public CurrentUserServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
    }

    private CurrentUserService CreateSutWithClaims(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);
        return new CurrentUserService(_httpContextAccessorMock.Object);
    }

    // =========================================================
    // Null / Unauthenticated Context Tests
    // =========================================================

    [Fact]
    public void Properties_WhenHttpContextIsNull_ReturnsNullAndFalse()
    {
        // Arrange
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);
        var sut = new CurrentUserService(_httpContextAccessorMock.Object);

        // Assert
        sut.Role.Should().BeNull();
        sut.UserId.Should().BeNull();
        sut.CustomerId.Should().BeNull();
        sut.StoreId.Should().BeNull();
        sut.StaffId.Should().BeNull();
        sut.IsAdmin.Should().BeFalse();
        sut.IsStaff.Should().BeFalse();
        sut.IsCustomer.Should().BeFalse();
    }

    [Fact]
    public void Properties_WhenUserHasNoClaims_ReturnsNullAndFalse()
    {
        // Arrange
        var sut = CreateSutWithClaims();

        // Assert
        sut.Role.Should().BeNull();
        sut.UserId.Should().BeNull();
        sut.CustomerId.Should().BeNull();
        sut.StoreId.Should().BeNull();
        sut.StaffId.Should().BeNull();
        sut.IsAdmin.Should().BeFalse();
        sut.IsStaff.Should().BeFalse();
        sut.IsCustomer.Should().BeFalse();
    }

    [Fact]
    public void Properties_WhenClaimsHaveNonIntegerValues_ReturnsNullForNumericIds()
    {
        // Arrange
        var sut = CreateSutWithClaims(
            new Claim(ClaimTypes.NameIdentifier, "invalid_id"),
            new Claim("customerId", "not_a_number"),
            new Claim("storeId", "xyz"),
            new Claim("staffId", "abc"));

        // Assert
        sut.UserId.Should().BeNull();
        sut.CustomerId.Should().BeNull();
        sut.StoreId.Should().BeNull();
        sut.StaffId.Should().BeNull();
    }

    // =========================================================
    // Admin Role Tests
    // =========================================================

    [Fact]
    public void Properties_WhenAdminUser_ParsesRoleAndUserIdCorrectly()
    {
        // Arrange
        var sut = CreateSutWithClaims(
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.NameIdentifier, "1"));

        // Assert
        sut.Role.Should().Be("Admin");
        sut.UserId.Should().Be(1);
        sut.IsAdmin.Should().BeTrue();
        sut.IsStaff.Should().BeFalse();
        sut.IsCustomer.Should().BeFalse();
        sut.CustomerId.Should().BeNull();
        sut.StoreId.Should().BeNull();
        sut.StaffId.Should().BeNull();
    }

    // =========================================================
    // Staff Role Tests
    // =========================================================

    [Fact]
    public void Properties_WhenStaffUser_ParsesStoreIdAndStaffIdCorrectly()
    {
        // Arrange
        var sut = CreateSutWithClaims(
            new Claim(ClaimTypes.Role, "Staff"),
            new Claim(ClaimTypes.NameIdentifier, "10"),
            new Claim("storeId", "2"),
            new Claim("staffId", "5"));

        // Assert
        sut.Role.Should().Be("Staff");
        sut.UserId.Should().Be(10);
        sut.StoreId.Should().Be(2);
        sut.StaffId.Should().Be(5);
        sut.IsAdmin.Should().BeFalse();
        sut.IsStaff.Should().BeTrue();
        sut.IsCustomer.Should().BeFalse();
        sut.CustomerId.Should().BeNull();
    }

    // =========================================================
    // Customer Role Tests
    // =========================================================

    [Fact]
    public void Properties_WhenCustomerUser_ParsesCustomerIdCorrectly()
    {
        // Arrange
        var sut = CreateSutWithClaims(
            new Claim(ClaimTypes.Role, "Customer"),
            new Claim(ClaimTypes.NameIdentifier, "25"),
            new Claim("customerId", "100"));

        // Assert
        sut.Role.Should().Be("Customer");
        sut.UserId.Should().Be(25);
        sut.CustomerId.Should().Be(100);
        sut.IsAdmin.Should().BeFalse();
        sut.IsStaff.Should().BeFalse();
        sut.IsCustomer.Should().BeTrue();
        sut.StoreId.Should().BeNull();
        sut.StaffId.Should().BeNull();
    }
}
