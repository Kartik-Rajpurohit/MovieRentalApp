using FluentAssertions;
using MockQueryable;
using MockQueryable.EntityFrameworkCore;
using Moq;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Users;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Services;
using Xunit;

namespace MovieRental.Tests.Unit.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _sut = new UserService(_userRepositoryMock.Object);
    }

    private static List<User> CreateSampleUsers()
    {
        var roleAdmin = new Role { RoleId = 1, RoleName = "Admin" };
        var roleCustomer = new Role { RoleId = 2, RoleName = "Customer" };

        return new List<User>
        {
            new()
            {
                UserId = 1,
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice@example.com",
                IsActive = true,
                RoleId = 1,
                Role = roleAdmin,
                Address = new Address { Street = "123 Broadway" }
            },
            new()
            {
                UserId = 2,
                FirstName = "Bob",
                LastName = "Jones",
                Email = "bob@example.com",
                IsActive = false,
                RoleId = 2,
                Role = roleCustomer,
                Address = new Address { Street = "456 Sunset Blvd" }
            },
            new()
            {
                UserId = 3,
                FirstName = "Charlie",
                LastName = "Brown",
                Email = "charlie@example.com",
                IsActive = true,
                RoleId = 2,
                Role = roleCustomer
            }
        };
    }

    // =========================================================
    // GetAllUsersAsync Tests
    // =========================================================

    [Fact]
    public async Task GetAllUsersAsync_WhenUsersExist_ReturnsUserList()
    {
        // Arrange
        var users = CreateSampleUsers();
        _userRepositoryMock.Setup(r => r.GetAllUsers()).Returns(users.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new UserFilterDto();

        // Act
        var result = await _sut.GetAllUsersAsync(pagination, filter);

        // Assert
        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(3);
        result.TotalPages.Should().Be(1);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllUsersAsync_WithSearchTermMatchingName_FiltersByName()
    {
        // Arrange
        var users = CreateSampleUsers();
        _userRepositoryMock.Setup(r => r.GetAllUsers()).Returns(users.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "alice" };
        var filter = new UserFilterDto();

        // Act
        var result = await _sut.GetAllUsersAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(u => u.Email == "alice@example.com");
    }

    [Fact]
    public async Task GetAllUsersAsync_WithSearchTermMatchingEmail_FiltersByEmail()
    {
        // Arrange
        var users = CreateSampleUsers();
        _userRepositoryMock.Setup(r => r.GetAllUsers()).Returns(users.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "bob@example.com" };
        var filter = new UserFilterDto();

        // Act
        var result = await _sut.GetAllUsersAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(u => u.UserId == 2);
    }

    [Fact]
    public async Task GetAllUsersAsync_WithRoleIdFilter_FiltersByRoleId()
    {
        // Arrange
        var users = CreateSampleUsers();
        _userRepositoryMock.Setup(r => r.GetAllUsers()).Returns(users.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new UserFilterDto { RoleId = 1 };

        // Act
        var result = await _sut.GetAllUsersAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(u => u.RoleName == "Admin");
    }

    [Fact]
    public async Task GetAllUsersAsync_WithIsActiveFilter_FiltersByActiveStatus()
    {
        // Arrange
        var users = CreateSampleUsers();
        _userRepositoryMock.Setup(r => r.GetAllUsers()).Returns(users.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new UserFilterDto { IsActive = false };

        // Act
        var result = await _sut.GetAllUsersAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(u => u.UserId == 2 && !u.IsActive);
    }

    [Theory]
    [InlineData("fullname", "asc", "Alice Smith")]
    [InlineData("email", "desc", "Charlie Brown")]
    [InlineData("default", "desc", "Charlie Brown")]
    public async Task GetAllUsersAsync_WithSorting_OrdersResultsCorrectly(string sortBy, string sortOrder, string expectedFirstFullName)
    {
        // Arrange
        var users = CreateSampleUsers();
        _userRepositoryMock.Setup(r => r.GetAllUsers()).Returns(users.BuildMock());

        var pagination = new PaginationInputDto
        {
            Page = 1,
            PageSize = 10,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var filter = new UserFilterDto();

        // Act
        var result = await _sut.GetAllUsersAsync(pagination, filter);

        // Assert
        result.Data.First().FullName.Should().Be(expectedFirstFullName);
    }

    [Fact]
    public async Task GetAllUsersAsync_WhenEmpty_ReturnsZeroTotals()
    {
        // Arrange
        var empty = new List<User>();
        _userRepositoryMock.Setup(r => r.GetAllUsers()).Returns(empty.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new UserFilterDto();

        // Act
        var result = await _sut.GetAllUsersAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    // =========================================================
    // GetUserByIdAsync Tests
    // =========================================================

    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ReturnsUserDto()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            FirstName = "Alice",
            LastName = "Smith",
            Email = "alice@example.com",
            IsActive = true,
            Role = new Role { RoleName = "Admin" }
        };
        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _sut.GetUserByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(1);
        result.Email.Should().Be("alice@example.com");
        result.RoleName.Should().Be("Admin");
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(999)).ReturnsAsync((User?)null);

        // Act
        var result = await _sut.GetUserByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================
    // UpdateUserAsync Tests
    // =========================================================

    [Fact]
    public async Task UpdateUserAsync_WhenUserNotFound_ReturnsNull()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(999)).ReturnsAsync((User?)null);

        var dto = new UpdateUserDto { UserId = 999, FirstName = "NewName" };

        // Act
        var result = await _sut.UpdateUserAsync(dto);

        // Assert
        result.Should().BeNull();
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_PatchFieldsOnly_UpdatesProvidedFields()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            FirstName = "Alice",
            LastName = "Smith",
            Email = "alice@example.com",
            AddressId = 1,
            RoleId = 1
        };

        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(1))
            .ReturnsAsync(user);

        _userRepositoryMock.Setup(r => r.GetRoleNameAsync(1))
            .ReturnsAsync("Admin");

        _userRepositoryMock.Setup(r => r.UpdateUserAsync(user))
            .ReturnsAsync(user);

        var dto = new UpdateUserDto
        {
            UserId = 1,
            FirstName = "Alicia",
            AddressId = 5
        };

        // Act
        var result = await _sut.UpdateUserAsync(dto);

        // Assert
        result.Should().NotBeNull();
        user.FirstName.Should().Be("Alicia");
        user.LastName.Should().Be("Smith"); // untouched
        user.AddressId.Should().Be(5);

        _userRepositoryMock.Verify(r => r.UpdateUserAsync(user), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenRoleChangesFromStaffToCustomer_DeletesOldStaffAndCreatesCustomer()
    {
        // Arrange
        var user = new User
        {
            UserId = 10,
            FirstName = "Staff",
            LastName = "Member",
            RoleId = 2
        };

        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(10)).ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.GetRoleNameAsync(2)).ReturnsAsync("Staff");
        _userRepositoryMock.Setup(r => r.GetRoleNameAsync(3)).ReturnsAsync("Customer");
        _userRepositoryMock.Setup(r => r.UpdateUserAsync(user)).ReturnsAsync(user);

        var dto = new UpdateUserDto
        {
            UserId = 10,
            RoleId = 3,
            StoreId = 1
        };

        // Act
        var result = await _sut.UpdateUserAsync(dto);

        // Assert
        result.Should().NotBeNull();
        user.RoleId.Should().Be(3);

        _userRepositoryMock.Verify(r => r.DeleteStaffByUserIdAsync(10), Times.Once);
        _userRepositoryMock.Verify(r => r.CreateCustomerAsync(10, 1), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenRoleChangesFromCustomerToStaff_DeletesOldCustomerAndCreatesStaff()
    {
        // Arrange
        var user = new User
        {
            UserId = 11,
            FirstName = "Customer",
            LastName = "User",
            RoleId = 3
        };

        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(11)).ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.GetRoleNameAsync(3)).ReturnsAsync("Customer");
        _userRepositoryMock.Setup(r => r.GetRoleNameAsync(2)).ReturnsAsync("Staff");
        _userRepositoryMock.Setup(r => r.UpdateUserAsync(user)).ReturnsAsync(user);

        var dto = new UpdateUserDto
        {
            UserId = 11,
            RoleId = 2,
            StoreId = 2
        };

        // Act
        var result = await _sut.UpdateUserAsync(dto);

        // Assert
        result.Should().NotBeNull();
        user.RoleId.Should().Be(2);

        _userRepositoryMock.Verify(r => r.DeleteCustomerByUserIdAsync(11), Times.Once);
        _userRepositoryMock.Verify(r => r.CreateStaffAsync(11, 2), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenRepositoryReturnsNull_ReturnsNull()
    {
        // Arrange
        var user = new User { UserId = 5 };
        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(5)).ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.UpdateUserAsync(user)).ReturnsAsync((User?)null);

        var dto = new UpdateUserDto { UserId = 5, FirstName = "Test" };

        // Act
        var result = await _sut.UpdateUserAsync(dto);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================
    // ToggleUserStatusAsync Tests
    // =========================================================

    [Fact]
    public async Task ToggleUserStatusAsync_WhenUserExists_TogglesStatusAndReturnsDto()
    {
        // Arrange
        var user = new User { UserId = 1, IsActive = false };
        _userRepositoryMock.Setup(r => r.ToggleUserStatusAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _sut.ToggleUserStatusAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(1);
        result.IsActive.Should().BeFalse();
        _userRepositoryMock.Verify(r => r.ToggleUserStatusAsync(1), Times.Once);
    }

    [Fact]
    public async Task ToggleUserStatusAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.ToggleUserStatusAsync(999)).ReturnsAsync((User?)null);

        // Act
        var result = await _sut.ToggleUserStatusAsync(999);

        // Assert
        result.Should().BeNull();
    }
}
