using FluentAssertions;
using MockQueryable;
using MockQueryable.EntityFrameworkCore;
using Moq;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Staff;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;
using MovieRental.Services.Services;
using Xunit;

namespace MovieRental.Tests.Unit.Services;

public class StaffServiceTests
{
    private readonly Mock<IStaffRepository> _staffRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly StaffService _sut;

    public StaffServiceTests()
    {
        _staffRepositoryMock = new Mock<IStaffRepository>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _sut = new StaffService(_staffRepositoryMock.Object, _currentUserMock.Object);
    }

    private static List<Staff> CreateSampleStaff()
    {
        var user1 = new User { UserId = 1, FirstName = "Mike", LastName = "Hillyer", Email = "mike@example.com", IsActive = true };
        var user2 = new User { UserId = 2, FirstName = "Jon", LastName = "Stephens", Email = "jon@example.com", IsActive = false };
        var user3 = new User { UserId = 3, FirstName = "Sarah", LastName = "Connor", Email = "sarah@example.com", IsActive = true };

        return new List<Staff>
        {
            new()
            {
                StaffId = 1,
                UserId = 1,
                User = user1,
                StoreId = 1
            },
            new()
            {
                StaffId = 2,
                UserId = 2,
                User = user2,
                StoreId = 2
            },
            new()
            {
                StaffId = 3,
                UserId = 3,
                User = user3,
                StoreId = 1
            }
        };
    }

    // =========================================================
    // GetAllStaffAsync Tests
    // =========================================================

    [Fact]
    public async Task GetAllStaffAsync_WhenStaffExists_ReturnsStaffList()
    {
        // Arrange
        var staffList = CreateSampleStaff();
        _staffRepositoryMock.Setup(r => r.GetAllStaff()).Returns(staffList.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new StaffFilterDto();

        // Act
        var result = await _sut.GetAllStaffAsync(pagination, filter);

        // Assert
        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(3);
        result.TotalPages.Should().Be(1);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllStaffAsync_WithSearchTermMatchingName_FiltersByName()
    {
        // Arrange
        var staffList = CreateSampleStaff();
        _staffRepositoryMock.Setup(r => r.GetAllStaff()).Returns(staffList.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "jon" };
        var filter = new StaffFilterDto();

        // Act
        var result = await _sut.GetAllStaffAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(s => s.FullName == "Jon Stephens");
    }

    [Fact]
    public async Task GetAllStaffAsync_WithSearchTermMatchingEmail_FiltersByEmail()
    {
        // Arrange
        var staffList = CreateSampleStaff();
        _staffRepositoryMock.Setup(r => r.GetAllStaff()).Returns(staffList.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "mike@example.com" };
        var filter = new StaffFilterDto();

        // Act
        var result = await _sut.GetAllStaffAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(s => s.StaffId == 1);
    }

    [Fact]
    public async Task GetAllStaffAsync_WithIsActiveFilter_FiltersByActiveStatus()
    {
        // Arrange
        var staffList = CreateSampleStaff();
        _staffRepositoryMock.Setup(r => r.GetAllStaff()).Returns(staffList.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new StaffFilterDto { IsActive = false };

        // Act
        var result = await _sut.GetAllStaffAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(s => s.StaffId == 2 && !s.IsActive);
    }

    [Fact]
    public async Task GetAllStaffAsync_WithStoreIdFilter_FiltersByStoreId()
    {
        // Arrange
        var staffList = CreateSampleStaff();
        _staffRepositoryMock.Setup(r => r.GetAllStaff()).Returns(staffList.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new StaffFilterDto { StoreId = 2 };

        // Act
        var result = await _sut.GetAllStaffAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(s => s.StoreId == 2);
    }

    [Theory]
    [InlineData("fullname", "asc", "Jon Stephens")]
    [InlineData("email", "desc", "Sarah Connor")]
    [InlineData("default", "desc", "Sarah Connor")]
    public async Task GetAllStaffAsync_WithSorting_OrdersResultsCorrectly(string sortBy, string sortOrder, string expectedFirstFullName)
    {
        // Arrange
        var staffList = CreateSampleStaff();
        _staffRepositoryMock.Setup(r => r.GetAllStaff()).Returns(staffList.BuildMock());

        var pagination = new PaginationInputDto
        {
            Page = 1,
            PageSize = 10,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var filter = new StaffFilterDto();

        // Act
        var result = await _sut.GetAllStaffAsync(pagination, filter);

        // Assert
        result.Data.First().FullName.Should().Be(expectedFirstFullName);
    }

    [Fact]
    public async Task GetAllStaffAsync_WhenEmpty_ReturnsZeroTotals()
    {
        // Arrange
        var empty = new List<Staff>();
        _staffRepositoryMock.Setup(r => r.GetAllStaff()).Returns(empty.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new StaffFilterDto();

        // Act
        var result = await _sut.GetAllStaffAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    // =========================================================
    // GetStaffByIdAsync Tests
    // =========================================================

    [Fact]
    public async Task GetStaffByIdAsync_WhenStaffExists_ReturnsStaffDetailDto()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            FirstName = "Mike",
            LastName = "Hillyer",
            Email = "mike@example.com",
            IsActive = true,
            Address = new Address { Street = "23 Work Way", Phone = "123-456" }
        };
        var staff = new Staff
        {
            StaffId = 1,
            UserId = 1,
            User = user,
            StoreId = 1
        };

        _staffRepositoryMock.Setup(r => r.GetStaffByIdAsync(1)).ReturnsAsync(staff);

        // Act
        var result = await _sut.GetStaffByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.StaffId.Should().Be(1);
        result.FullName.Should().Be("Mike Hillyer");
        result.Email.Should().Be("mike@example.com");
        result.StoreId.Should().Be(1);
        result.Street.Should().Be("23 Work Way");
    }

    [Fact]
    public async Task GetStaffByIdAsync_WhenStaffDoesNotExist_ReturnsNull()
    {
        // Arrange
        _staffRepositoryMock.Setup(r => r.GetStaffByIdAsync(999)).ReturnsAsync((Staff?)null);

        // Act
        var result = await _sut.GetStaffByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllStaffAsync_WhenStaff_ScopesToStaffAssignedStore()
    {
        // Arrange
        var staffList = CreateSampleStaff();
        _staffRepositoryMock.Setup(r => r.GetAllStaff()).Returns(staffList.BuildMock());
        _currentUserMock.Setup(u => u.IsStaff).Returns(true);
        _currentUserMock.Setup(u => u.StoreId).Returns(2);

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new StaffFilterDto();

        // Act
        var result = await _sut.GetAllStaffAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.First().StaffId.Should().Be(2);
    }

    [Fact]
    public async Task GetAllStaffAsync_WhenStaffHasNoStoreId_ReturnsEmpty()
    {
        // Arrange
        var staffList = CreateSampleStaff();
        _staffRepositoryMock.Setup(r => r.GetAllStaff()).Returns(staffList.BuildMock());
        _currentUserMock.Setup(u => u.IsStaff).Returns(true);
        _currentUserMock.Setup(u => u.StoreId).Returns((int?)null);

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new StaffFilterDto();

        // Act
        var result = await _sut.GetAllStaffAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllStaffAsync_WhenCustomer_ReturnsEmpty()
    {
        // Arrange
        var staffList = CreateSampleStaff();
        _staffRepositoryMock.Setup(r => r.GetAllStaff()).Returns(staffList.BuildMock());
        _currentUserMock.Setup(u => u.IsCustomer).Returns(true);

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new StaffFilterDto();

        // Act
        var result = await _sut.GetAllStaffAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStaffByIdAsync_WhenCustomerAttemptsToViewStaff_ReturnsNull()
    {
        // Arrange
        var staff = CreateSampleStaff().First(s => s.StaffId == 1);
        _staffRepositoryMock.Setup(r => r.GetStaffByIdAsync(1)).ReturnsAsync(staff);
        _currentUserMock.Setup(u => u.IsCustomer).Returns(true);

        // Act
        var result = await _sut.GetStaffByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetStaffByIdAsync_WhenStaffAttemptsToViewOtherStoreStaff_ReturnsNull()
    {
        // Arrange
        var staff = CreateSampleStaff().First(s => s.StaffId == 1); // StoreId = 1
        _staffRepositoryMock.Setup(r => r.GetStaffByIdAsync(1)).ReturnsAsync(staff);
        _currentUserMock.Setup(u => u.IsStaff).Returns(true);
        _currentUserMock.Setup(u => u.StoreId).Returns(2); // Assigned to Store 2

        // Act
        var result = await _sut.GetStaffByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }
}
