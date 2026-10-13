using FluentAssertions;
using MockQueryable;
using MockQueryable.EntityFrameworkCore;
using Moq;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Customers;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;
using MovieRental.Services.Services;
using Xunit;

namespace MovieRental.Tests.Unit.Services;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly CustomerService _sut;

    public CustomerServiceTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _sut = new CustomerService(_customerRepositoryMock.Object, _currentUserMock.Object);
    }

    private static List<Customer> CreateSampleCustomers()
    {
        var user1 = new User { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", IsActive = true };
        var user2 = new User { UserId = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", IsActive = false };
        var user3 = new User { UserId = 3, FirstName = "Bob", LastName = "Taylor", Email = "bob@example.com", IsActive = true };

        return new List<Customer>
        {
            new()
            {
                CustomerId = 1,
                UserId = 1,
                User = user1,
                StoreId = 1,
                CreateDate = new DateOnly(2025, 1, 1)
            },
            new()
            {
                CustomerId = 2,
                UserId = 2,
                User = user2,
                StoreId = 2,
                CreateDate = new DateOnly(2025, 1, 2)
            },
            new()
            {
                CustomerId = 3,
                UserId = 3,
                User = user3,
                StoreId = 1,
                CreateDate = new DateOnly(2025, 1, 3)
            }
        };
    }

    // =========================================================
    // GetAllCustomersAsync Tests
    // =========================================================

    [Fact]
    public async Task GetAllCustomersAsync_WhenCustomersExist_ReturnsCustomerList()
    {
        // Arrange
        var customers = CreateSampleCustomers();
        _customerRepositoryMock.Setup(r => r.GetAllCustomers()).Returns(customers.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new CustomerFilterDto();

        // Act
        var result = await _sut.GetAllCustomersAsync(pagination, filter);

        // Assert
        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(3);
        result.TotalPages.Should().Be(1);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllCustomersAsync_WithSearchTermMatchingName_FiltersByName()
    {
        // Arrange
        var customers = CreateSampleCustomers();
        _customerRepositoryMock.Setup(r => r.GetAllCustomers()).Returns(customers.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "jane" };
        var filter = new CustomerFilterDto();

        // Act
        var result = await _sut.GetAllCustomersAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(c => c.FullName == "Jane Smith");
    }

    [Fact]
    public async Task GetAllCustomersAsync_WithSearchTermMatchingEmail_FiltersByEmail()
    {
        // Arrange
        var customers = CreateSampleCustomers();
        _customerRepositoryMock.Setup(r => r.GetAllCustomers()).Returns(customers.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "john@example.com" };
        var filter = new CustomerFilterDto();

        // Act
        var result = await _sut.GetAllCustomersAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(c => c.CustomerId == 1);
    }

    [Fact]
    public async Task GetAllCustomersAsync_WithIsActiveFilter_FiltersByActiveStatus()
    {
        // Arrange
        var customers = CreateSampleCustomers();
        _customerRepositoryMock.Setup(r => r.GetAllCustomers()).Returns(customers.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new CustomerFilterDto { IsActive = false };

        // Act
        var result = await _sut.GetAllCustomersAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(c => c.CustomerId == 2 && !c.IsActive);
    }

    [Fact]
    public async Task GetAllCustomersAsync_WithStoreIdFilter_FiltersByStoreId()
    {
        // Arrange
        var customers = CreateSampleCustomers();
        _customerRepositoryMock.Setup(r => r.GetAllCustomers()).Returns(customers.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new CustomerFilterDto { StoreId = 2 };

        // Act
        var result = await _sut.GetAllCustomersAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(c => c.StoreId == 2);
    }

    [Theory]
    [InlineData("fullname", "asc", "Bob Taylor")]
    [InlineData("email", "desc", "John Doe")]
    [InlineData("default", "desc", "Bob Taylor")]
    public async Task GetAllCustomersAsync_WithSorting_OrdersResultsCorrectly(string sortBy, string sortOrder, string expectedFirstFullName)
    {
        // Arrange
        var customers = CreateSampleCustomers();
        _customerRepositoryMock.Setup(r => r.GetAllCustomers()).Returns(customers.BuildMock());

        var pagination = new PaginationInputDto
        {
            Page = 1,
            PageSize = 10,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var filter = new CustomerFilterDto();

        // Act
        var result = await _sut.GetAllCustomersAsync(pagination, filter);

        // Assert
        result.Data.First().FullName.Should().Be(expectedFirstFullName);
    }

    [Fact]
    public async Task GetAllCustomersAsync_WhenEmpty_ReturnsZeroTotals()
    {
        // Arrange
        var empty = new List<Customer>();
        _customerRepositoryMock.Setup(r => r.GetAllCustomers()).Returns(empty.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new CustomerFilterDto();

        // Act
        var result = await _sut.GetAllCustomersAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    // =========================================================
    // GetCustomerByIdAsync Tests
    // =========================================================

    [Fact]
    public async Task GetCustomerByIdAsync_WhenCustomerExists_ReturnsCustomerDetailDto()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            IsActive = true,
            Address = new Address { Street = "123 Main St", Phone = "555-0100" }
        };
        var customer = new Customer
        {
            CustomerId = 1,
            UserId = 1,
            User = user,
            StoreId = 1,
            CreateDate = new DateOnly(2025, 1, 1)
        };

        _customerRepositoryMock.Setup(r => r.GetCustomerByIdAsync(1)).ReturnsAsync(customer);

        // Act
        var result = await _sut.GetCustomerByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.CustomerId.Should().Be(1);
        result.FullName.Should().Be("John Doe");
        result.Email.Should().Be("john@example.com");
        result.StoreId.Should().Be(1);
        result.Street.Should().Be("123 Main St");
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WhenCustomerDoesNotExist_ReturnsNull()
    {
        // Arrange
        _customerRepositoryMock.Setup(r => r.GetCustomerByIdAsync(999)).ReturnsAsync((Customer?)null);

        // Act
        var result = await _sut.GetCustomerByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllCustomersAsync_WhenStaff_ScopesToStaffAssignedStore()
    {
        // Arrange
        var customers = CreateSampleCustomers();
        _customerRepositoryMock.Setup(r => r.GetAllCustomers()).Returns(customers.BuildMock());
        _currentUserMock.Setup(u => u.IsStaff).Returns(true);
        _currentUserMock.Setup(u => u.StoreId).Returns(2);

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new CustomerFilterDto();

        // Act
        var result = await _sut.GetAllCustomersAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.First().CustomerId.Should().Be(2);
    }

    [Fact]
    public async Task GetAllCustomersAsync_WhenStaffHasNoStoreId_ReturnsEmpty()
    {
        // Arrange
        var customers = CreateSampleCustomers();
        _customerRepositoryMock.Setup(r => r.GetAllCustomers()).Returns(customers.BuildMock());
        _currentUserMock.Setup(u => u.IsStaff).Returns(true);
        _currentUserMock.Setup(u => u.StoreId).Returns((int?)null);

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new CustomerFilterDto();

        // Act
        var result = await _sut.GetAllCustomersAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WhenCustomerAttemptsToViewOtherCustomer_ReturnsNull()
    {
        // Arrange
        var customer = CreateSampleCustomers().First(c => c.CustomerId == 1);
        _customerRepositoryMock.Setup(r => r.GetCustomerByIdAsync(1)).ReturnsAsync(customer);
        _currentUserMock.Setup(u => u.IsCustomer).Returns(true);
        _currentUserMock.Setup(u => u.CustomerId).Returns(999); // Different customer ID

        // Act
        var result = await _sut.GetCustomerByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WhenStaffAttemptsToViewOtherStoreCustomer_ReturnsNull()
    {
        // Arrange
        var customer = CreateSampleCustomers().First(c => c.CustomerId == 1); // StoreId = 1
        _customerRepositoryMock.Setup(r => r.GetCustomerByIdAsync(1)).ReturnsAsync(customer);
        _currentUserMock.Setup(u => u.IsStaff).Returns(true);
        _currentUserMock.Setup(u => u.StoreId).Returns(2); // StoreId = 2

        // Act
        var result = await _sut.GetCustomerByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }
}
