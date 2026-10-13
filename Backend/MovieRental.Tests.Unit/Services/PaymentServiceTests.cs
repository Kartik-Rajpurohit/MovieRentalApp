using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable;
using MockQueryable.EntityFrameworkCore;
using Moq;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Payments;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;
using MovieRental.Services.Services;
using Xunit;

namespace MovieRental.Tests.Unit.Services;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IRentalRepository> _rentalRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<ILogger<PaymentService>> _loggerMock;
    private readonly PaymentService _sut;

    public PaymentServiceTests()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _rentalRepositoryMock = new Mock<IRentalRepository>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<PaymentService>>();

        _sut = new PaymentService(
            _paymentRepositoryMock.Object,
            _rentalRepositoryMock.Object,
            _currentUserMock.Object,
            _loggerMock.Object);
    }

    private static List<Payment> CreateSamplePayments()
    {
        var movie1 = new Movie { MovieId = 1, Title = "Inception" };
        var movie2 = new Movie { MovieId = 2, Title = "The Dark Knight" };

        var inv1 = new Inventory { InventoryId = 1, MovieId = 1, Movie = movie1, StoreId = 1 };
        var inv2 = new Inventory { InventoryId = 2, MovieId = 2, Movie = movie2, StoreId = 2 };

        var rental1 = new Rental { RentalId = 10, Inventory = inv1, CustomerId = 1 };
        var rental2 = new Rental { RentalId = 20, Inventory = inv2, CustomerId = 2 };

        var userCust1 = new User { FirstName = "John", LastName = "Doe" };
        var userCust2 = new User { FirstName = "Jane", LastName = "Smith" };
        var cust1 = new Customer { CustomerId = 1, User = userCust1 };
        var cust2 = new Customer { CustomerId = 2, User = userCust2 };

        var userStaff1 = new User { FirstName = "Mike", LastName = "Ross" };
        var staff1 = new Staff { StaffId = 1, StoreId = 1, User = userStaff1 };
        var staff2 = new Staff { StaffId = 2, StoreId = 2, User = userStaff1 };

        return new List<Payment>
        {
            new()
            {
                PaymentId = 1,
                CustomerId = 1,
                Customer = cust1,
                StaffId = 1,
                Staff = staff1,
                RentalId = 10,
                Rental = rental1,
                Amount = 5.99m,
                PaymentDate = new DateTime(2025, 1, 5, 10, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                PaymentId = 2,
                CustomerId = 2,
                Customer = cust2,
                StaffId = 2,
                Staff = staff2,
                RentalId = 20,
                Rental = rental2,
                Amount = 15.00m,
                PaymentDate = new DateTime(2025, 1, 10, 10, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                PaymentId = 3,
                CustomerId = 1,
                Customer = cust1,
                StaffId = 1,
                Staff = staff1,
                RentalId = 10,
                Rental = rental1,
                Amount = 2.50m,
                PaymentDate = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc)
            }
        };
    }

    // =========================================================
    // GetAllPaymentsAsync Tests
    // =========================================================

    [Fact]
    public async Task GetAllPaymentsAsync_WhenAdmin_ReturnsAllPayments()
    {
        // Arrange
        var payments = CreateSamplePayments();
        _paymentRepositoryMock.Setup(r => r.GetAllPayments()).Returns(payments.BuildMock());
        _currentUserMock.Setup(u => u.IsCustomer).Returns(false);
        _currentUserMock.Setup(u => u.IsStaff).Returns(false);

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new PaymentFilterDto();

        // Act
        var result = await _sut.GetAllPaymentsAsync(pagination, filter);

        // Assert
        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(3);
        result.Data.Should().HaveCount(3);
        result.Data.First().MovieTitle.Should().Be("Inception");
    }

    [Fact]
    public async Task GetAllPaymentsAsync_WhenCustomer_ScopesToCallerCustomerId()
    {
        // Arrange
        var payments = CreateSamplePayments();
        _paymentRepositoryMock.Setup(r => r.GetAllPayments()).Returns(payments.BuildMock());
        _currentUserMock.Setup(u => u.IsCustomer).Returns(true);
        _currentUserMock.Setup(u => u.CustomerId).Returns(2);

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new PaymentFilterDto();

        // Act
        var result = await _sut.GetAllPaymentsAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().OnlyContain(p => p.CustomerId == 2);
    }

    [Fact]
    public async Task GetAllPaymentsAsync_WhenCustomerHasNoCustomerId_ReturnsEmpty()
    {
        // Arrange
        _currentUserMock.Setup(u => u.IsCustomer).Returns(true);
        _currentUserMock.Setup(u => u.CustomerId).Returns((int?)null);

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new PaymentFilterDto();

        // Act
        var result = await _sut.GetAllPaymentsAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllPaymentsAsync_WhenStaff_ScopesToStaffAssignedStore()
    {
        // Arrange
        var payments = CreateSamplePayments();
        _paymentRepositoryMock.Setup(r => r.GetAllPayments()).Returns(payments.BuildMock());
        _currentUserMock.Setup(u => u.IsCustomer).Returns(false);
        _currentUserMock.Setup(u => u.IsStaff).Returns(true);
        _currentUserMock.Setup(u => u.StoreId).Returns(2);

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new PaymentFilterDto();

        // Act
        var result = await _sut.GetAllPaymentsAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.First().PaymentId.Should().Be(2);
    }

    [Fact]
    public async Task GetAllPaymentsAsync_WhenStaffHasNoStoreId_ReturnsEmpty()
    {
        // Arrange
        var payments = CreateSamplePayments();
        _paymentRepositoryMock.Setup(r => r.GetAllPayments()).Returns(payments.BuildMock());
        _currentUserMock.Setup(u => u.IsCustomer).Returns(false);
        _currentUserMock.Setup(u => u.IsStaff).Returns(true);
        _currentUserMock.Setup(u => u.StoreId).Returns((int?)null);

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new PaymentFilterDto();

        // Act
        var result = await _sut.GetAllPaymentsAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Dark Knight", 1)]
    [InlineData("John", 2)]
    [InlineData("2", 1)] // Search by payment ID
    [InlineData("Unknown", 0)]
    public async Task GetAllPaymentsAsync_WithSearch_FiltersAcrossFields(string search, int expectedCount)
    {
        // Arrange
        var payments = CreateSamplePayments();
        _paymentRepositoryMock.Setup(r => r.GetAllPayments()).Returns(payments.BuildMock());
        _currentUserMock.Setup(u => u.IsCustomer).Returns(false);
        _currentUserMock.Setup(u => u.IsStaff).Returns(false);

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = search };
        var filter = new PaymentFilterDto();

        // Act
        var result = await _sut.GetAllPaymentsAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(expectedCount);
    }

    [Fact]
    public async Task GetAllPaymentsAsync_WithAmountAndDateFilters_FiltersResults()
    {
        // Arrange
        var payments = CreateSamplePayments();
        _paymentRepositoryMock.Setup(r => r.GetAllPayments()).Returns(payments.BuildMock());
        _currentUserMock.Setup(u => u.IsCustomer).Returns(false);
        _currentUserMock.Setup(u => u.IsStaff).Returns(false);

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new PaymentFilterDto
        {
            MinAmount = 10.00m,
            FromDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var result = await _sut.GetAllPaymentsAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.First().PaymentId.Should().Be(2);
    }

    [Theory]
    [InlineData("amount", "asc", 3)]      // 2.50m (id 3)
    [InlineData("paymentdate", "desc", 3)] // 2025-01-15 (id 3)
    [InlineData("default", "desc", 3)]     // PaymentId desc (id 3)
    public async Task GetAllPaymentsAsync_WithSorting_OrdersResultsCorrectly(string sortBy, string sortOrder, int expectedFirstId)
    {
        // Arrange
        var payments = CreateSamplePayments();
        _paymentRepositoryMock.Setup(r => r.GetAllPayments()).Returns(payments.BuildMock());
        _currentUserMock.Setup(u => u.IsCustomer).Returns(false);
        _currentUserMock.Setup(u => u.IsStaff).Returns(false);

        var pagination = new PaginationInputDto
        {
            Page = 1,
            PageSize = 10,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var filter = new PaymentFilterDto();

        // Act
        var result = await _sut.GetAllPaymentsAsync(pagination, filter);

        // Assert
        result.Data.First().PaymentId.Should().Be(expectedFirstId);
    }

    [Fact]
    public async Task GetAllPaymentsAsync_WhenEmpty_ReturnsZeroTotals()
    {
        // Arrange
        var empty = new List<Payment>();
        _paymentRepositoryMock.Setup(r => r.GetAllPayments()).Returns(empty.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new PaymentFilterDto();

        // Act
        var result = await _sut.GetAllPaymentsAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    // =========================================================
    // GetPaymentByIdAsync Tests
    // =========================================================

    [Fact]
    public async Task GetPaymentByIdAsync_WhenExistsAndAuthorized_ReturnsDto()
    {
        // Arrange
        var payment = CreateSamplePayments().First(p => p.PaymentId == 1);
        _paymentRepositoryMock.Setup(r => r.GetPaymentByIdAsync(1)).ReturnsAsync(payment);
        _currentUserMock.Setup(u => u.IsCustomer).Returns(false);
        _currentUserMock.Setup(u => u.IsStaff).Returns(false);

        // Act
        var result = await _sut.GetPaymentByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.PaymentId.Should().Be(1);
        result.Amount.Should().Be(5.99m);
        result.CustomerName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetPaymentByIdAsync_WhenCustomerAttemptsToViewOtherCustomerPayment_ReturnsNull()
    {
        // Arrange
        var payment = CreateSamplePayments().First(p => p.PaymentId == 1); // CustomerId = 1
        _paymentRepositoryMock.Setup(r => r.GetPaymentByIdAsync(1)).ReturnsAsync(payment);
        _currentUserMock.Setup(u => u.IsCustomer).Returns(true);
        _currentUserMock.Setup(u => u.CustomerId).Returns(999); // Different customer

        // Act
        var result = await _sut.GetPaymentByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPaymentByIdAsync_WhenStaffAttemptsToViewOtherStorePayment_ReturnsNull()
    {
        // Arrange
        var payment = CreateSamplePayments().First(p => p.PaymentId == 1); // Staff StoreId = 1
        _paymentRepositoryMock.Setup(r => r.GetPaymentByIdAsync(1)).ReturnsAsync(payment);
        _currentUserMock.Setup(u => u.IsCustomer).Returns(false);
        _currentUserMock.Setup(u => u.IsStaff).Returns(true);
        _currentUserMock.Setup(u => u.StoreId).Returns(2); // Different store

        // Act
        var result = await _sut.GetPaymentByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPaymentByIdAsync_WhenStaffHasNoStoreId_ReturnsNull()
    {
        // Arrange
        var payment = CreateSamplePayments().First(p => p.PaymentId == 1);
        _paymentRepositoryMock.Setup(r => r.GetPaymentByIdAsync(1)).ReturnsAsync(payment);
        _currentUserMock.Setup(u => u.IsCustomer).Returns(false);
        _currentUserMock.Setup(u => u.IsStaff).Returns(true);
        _currentUserMock.Setup(u => u.StoreId).Returns((int?)null);

        // Act
        var result = await _sut.GetPaymentByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPaymentByIdAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        _paymentRepositoryMock.Setup(r => r.GetPaymentByIdAsync(999)).ReturnsAsync((Payment?)null);

        // Act
        var result = await _sut.GetPaymentByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================
    // CreatePaymentAsync Tests
    // =========================================================

    [Fact]
    public async Task CreatePaymentAsync_WhenValid_CreatesAndReturnsDto()
    {
        // Arrange
        var dto = new CreatePaymentDto { RentalId = 10, CustomerId = 1, StaffId = 1, Amount = 9.99m };
        var rental = new Rental
        {
            RentalId = 10,
            CustomerId = 1,
            Inventory = new Inventory { StoreId = 1, Movie = new Movie { Title = "Inception" } },
            Staff = new Staff { StoreId = 1 }
        };

        _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(10)).ReturnsAsync(rental);
        _rentalRepositoryMock.Setup(r => r.CustomerExistsAsync(1)).ReturnsAsync(true);
        _rentalRepositoryMock.Setup(r => r.StaffExistsAsync(1)).ReturnsAsync(true);

        _currentUserMock.Setup(u => u.IsStaff).Returns(false);

        var created = new Payment
        {
            PaymentId = 50,
            RentalId = 10,
            Rental = rental,
            CustomerId = 1,
            Customer = new Customer { CustomerId = 1, User = new User { FirstName = "John", LastName = "Doe" } },
            StaffId = 1,
            Staff = new Staff { StaffId = 1, User = new User { FirstName = "Mike", LastName = "Ross" } },
            Amount = 9.99m,
            PaymentDate = DateTime.UtcNow
        };
        _paymentRepositoryMock.Setup(r => r.CreatePaymentAsync(It.IsAny<Payment>())).ReturnsAsync(created);

        // Act
        var result = await _sut.CreatePaymentAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.PaymentId.Should().Be(50);
        result.Amount.Should().Be(9.99m);
        result.MovieTitle.Should().Be("Inception");
    }

    [Fact]
    public async Task CreatePaymentAsync_WhenAmountIsZeroOrNegative_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreatePaymentDto { Amount = 0m, RentalId = 1, CustomerId = 1, StaffId = 1 };

        // Act
        var act = () => _sut.CreatePaymentAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Payment amount must be greater than zero*");
    }

    [Fact]
    public async Task CreatePaymentAsync_WhenRentalNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreatePaymentDto { Amount = 10m, RentalId = 999, CustomerId = 1, StaffId = 1 };
        _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(999)).ReturnsAsync((Rental?)null);

        // Act
        var act = () => _sut.CreatePaymentAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Rental #999 does not exist*");
    }

    [Fact]
    public async Task CreatePaymentAsync_WhenCustomerMismatch_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreatePaymentDto { Amount = 10m, RentalId = 1, CustomerId = 2, StaffId = 1 };
        var rental = new Rental { RentalId = 1, CustomerId = 1 };
        _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(1)).ReturnsAsync(rental);

        // Act
        var act = () => _sut.CreatePaymentAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Customer #2 does not match the customer on rental record #1*");
    }

    [Fact]
    public async Task CreatePaymentAsync_WhenCustomerDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreatePaymentDto { Amount = 10m, RentalId = 1, CustomerId = 1, StaffId = 1 };
        var rental = new Rental { RentalId = 1, CustomerId = 1 };
        _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(1)).ReturnsAsync(rental);
        _rentalRepositoryMock.Setup(r => r.CustomerExistsAsync(1)).ReturnsAsync(false);

        // Act
        var act = () => _sut.CreatePaymentAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Customer #1 does not exist or has been deleted*");
    }

    [Fact]
    public async Task CreatePaymentAsync_WhenStaffStoreMismatch_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreatePaymentDto { Amount = 10m, RentalId = 1, CustomerId = 1, StaffId = 1 };
        var rental = new Rental
        {
            RentalId = 1,
            CustomerId = 1,
            Inventory = new Inventory { StoreId = 2 },
            Staff = new Staff { StoreId = 2 }
        };
        _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(1)).ReturnsAsync(rental);
        _rentalRepositoryMock.Setup(r => r.CustomerExistsAsync(1)).ReturnsAsync(true);
        _rentalRepositoryMock.Setup(r => r.StaffExistsAsync(1)).ReturnsAsync(true);

        _currentUserMock.Setup(u => u.IsStaff).Returns(true);
        _currentUserMock.Setup(u => u.StoreId).Returns(1); // Staff belongs to store 1, rental is in store 2

        // Act
        var act = () => _sut.CreatePaymentAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Staff can only process payments for rentals belonging to their assigned store*");
    }

    [Fact]
    public async Task CreatePaymentAsync_WhenStaffHasNoStoreId_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreatePaymentDto { Amount = 10m, RentalId = 1, CustomerId = 1, StaffId = 1 };
        var rental = new Rental
        {
            RentalId = 1,
            CustomerId = 1,
            Inventory = new Inventory { StoreId = 1 },
            Staff = new Staff { StoreId = 1 }
        };
        _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(1)).ReturnsAsync(rental);
        _rentalRepositoryMock.Setup(r => r.CustomerExistsAsync(1)).ReturnsAsync(true);
        _rentalRepositoryMock.Setup(r => r.StaffExistsAsync(1)).ReturnsAsync(true);

        _currentUserMock.Setup(u => u.IsStaff).Returns(true);
        _currentUserMock.Setup(u => u.StoreId).Returns((int?)null); // Unassigned staff

        // Act
        var act = () => _sut.CreatePaymentAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Staff can only process payments for rentals belonging to their assigned store*");
    }
}
