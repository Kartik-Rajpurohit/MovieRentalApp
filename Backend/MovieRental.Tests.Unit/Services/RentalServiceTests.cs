using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable;
using MockQueryable.EntityFrameworkCore;
using Moq;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Rentals;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;
using MovieRental.Services.Services;
using Xunit;

namespace MovieRental.Tests.Unit.Services;

public class RentalServiceTests
{
    private readonly Mock<IRentalRepository> _rentalRepositoryMock;
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<ILogger<RentalService>> _loggerMock;
    private readonly RentalService _sut;

    public RentalServiceTests()
    {
        _rentalRepositoryMock = new Mock<IRentalRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<RentalService>>();

        _sut = new RentalService(
            _rentalRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            _currentUserMock.Object,
            _loggerMock.Object);
    }

    private static List<Rental> CreateSampleRentals()
    {
        var movie1 = new Movie { MovieId = 1, Title = "Inception", RentalRate = 2.99m };
        var movie2 = new Movie { MovieId = 2, Title = "The Dark Knight", RentalRate = 3.99m };

        var userCust1 = new User { FirstName = "John", LastName = "Doe" };
        var userCust2 = new User { FirstName = "Jane", LastName = "Smith" };
        var customer1 = new Customer { CustomerId = 1, User = userCust1 };
        var customer2 = new Customer { CustomerId = 2, User = userCust2 };

        var userStaff1 = new User { FirstName = "Mike", LastName = "Ross" };
        var staff1 = new Staff { StaffId = 1, StoreId = 1, User = userStaff1 };
        var staff2 = new Staff { StaffId = 2, StoreId = 2, User = userStaff1 };

        var inv1 = new Inventory { InventoryId = 1, MovieId = 1, Movie = movie1, StoreId = 1 };
        var inv2 = new Inventory { InventoryId = 2, MovieId = 2, Movie = movie2, StoreId = 2 };

        return new List<Rental>
        {
            new()
            {
                RentalId = 1,
                RentalDate = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                ReturnDate = new DateTime(2025, 1, 5, 10, 0, 0, DateTimeKind.Utc),
                InventoryId = 1,
                Inventory = inv1,
                CustomerId = 1,
                Customer = customer1,
                StaffId = 1,
                Staff = staff1,
                Payments = new List<Payment> { new() { PaymentId = 1, Amount = 11.96m } }
            },
            new()
            {
                RentalId = 2,
                RentalDate = new DateTime(2025, 1, 10, 10, 0, 0, DateTimeKind.Utc),
                ReturnDate = null, // Active unreturned rental
                InventoryId = 2,
                Inventory = inv2,
                CustomerId = 2,
                Customer = customer2,
                StaffId = 2,
                Staff = staff2,
                Payments = new List<Payment>()
            },
            new()
            {
                RentalId = 3,
                RentalDate = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc),
                ReturnDate = null,
                InventoryId = 1,
                Inventory = inv1,
                CustomerId = 1,
                Customer = customer1,
                StaffId = 1,
                Staff = staff1,
                Payments = new List<Payment>()
            }
        };
    }

    // =========================================================
    // GetAllRentalsAsync Tests
    // =========================================================

    [Fact]
    public async Task GetAllRentalsAsync_WhenAdmin_ReturnsAllRentals()
    {
        // Arrange
        var rentals = CreateSampleRentals();
        _rentalRepositoryMock.Setup(r => r.GetAllRentals()).Returns(rentals.BuildMock());
        _currentUserMock.Setup(u => u.IsCustomer).Returns(false);
        _currentUserMock.Setup(u => u.IsStaff).Returns(false);

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new RentalFilterDto();

        // Act
        var result = await _sut.GetAllRentalsAsync(pagination, filter);

        // Assert
        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(3);
        result.Data.Should().HaveCount(3);
        result.Data.First().MovieTitle.Should().Be("Inception");
    }

    [Fact]
    public async Task GetAllRentalsAsync_WhenCustomer_ScopesToCallerCustomerId()
    {
        // Arrange
        var rentals = CreateSampleRentals();
        _rentalRepositoryMock.Setup(r => r.GetAllRentals()).Returns(rentals.BuildMock());
        _currentUserMock.Setup(u => u.IsCustomer).Returns(true);
        _currentUserMock.Setup(u => u.CustomerId).Returns(2);

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new RentalFilterDto();

        // Act
        var result = await _sut.GetAllRentalsAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().OnlyContain(r => r.CustomerId == 2);
    }

    [Fact]
    public async Task GetAllRentalsAsync_WhenCustomerHasNoCustomerId_ReturnsEmpty()
    {
        // Arrange
        _currentUserMock.Setup(u => u.IsCustomer).Returns(true);
        _currentUserMock.Setup(u => u.CustomerId).Returns((int?)null);

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new RentalFilterDto();

        // Act
        var result = await _sut.GetAllRentalsAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllRentalsAsync_WhenStaff_ScopesToStaffAssignedStore()
    {
        // Arrange
        var rentals = CreateSampleRentals();
        _rentalRepositoryMock.Setup(r => r.GetAllRentals()).Returns(rentals.BuildMock());
        _currentUserMock.Setup(u => u.IsCustomer).Returns(false);
        _currentUserMock.Setup(u => u.IsStaff).Returns(true);
        _currentUserMock.Setup(u => u.StoreId).Returns(2);

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new RentalFilterDto();

        // Act
        var result = await _sut.GetAllRentalsAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.First().RentalId.Should().Be(2);
    }

    [Theory]
    [InlineData("Inception", 2)]
    [InlineData("Jane", 1)]
    [InlineData("2", 1)] // Search by rental ID
    [InlineData("Unknown", 0)]
    public async Task GetAllRentalsAsync_WithSearch_FiltersAcrossFields(string search, int expectedCount)
    {
        // Arrange
        var rentals = CreateSampleRentals();
        _rentalRepositoryMock.Setup(r => r.GetAllRentals()).Returns(rentals.BuildMock());
        _currentUserMock.Setup(u => u.IsCustomer).Returns(false);
        _currentUserMock.Setup(u => u.IsStaff).Returns(false);

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = search };
        var filter = new RentalFilterDto();

        // Act
        var result = await _sut.GetAllRentalsAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(expectedCount);
    }

    [Fact]
    public async Task GetAllRentalsAsync_WithStatusFilters_FiltersByReturnAndPayment()
    {
        // Arrange
        var rentals = CreateSampleRentals();
        _rentalRepositoryMock.Setup(r => r.GetAllRentals()).Returns(rentals.BuildMock());
        _currentUserMock.Setup(u => u.IsCustomer).Returns(false);
        _currentUserMock.Setup(u => u.IsStaff).Returns(false);

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new RentalFilterDto
        {
            IsReturned = true,
            HasPayment = true
        };

        // Act
        var result = await _sut.GetAllRentalsAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.First().RentalId.Should().Be(1);
    }

    [Theory]
    [InlineData("rentaldate", "asc", 1)]
    [InlineData("movietitle", "desc", 2)] // The Dark Knight (id 2) vs Inception
    [InlineData("default", "desc", 3)]    // RentalId desc
    public async Task GetAllRentalsAsync_WithSorting_OrdersResultsCorrectly(string sortBy, string sortOrder, int expectedFirstId)
    {
        // Arrange
        var rentals = CreateSampleRentals();
        _rentalRepositoryMock.Setup(r => r.GetAllRentals()).Returns(rentals.BuildMock());
        _currentUserMock.Setup(u => u.IsCustomer).Returns(false);
        _currentUserMock.Setup(u => u.IsStaff).Returns(false);

        var pagination = new PaginationInputDto
        {
            Page = 1,
            PageSize = 10,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var filter = new RentalFilterDto();

        // Act
        var result = await _sut.GetAllRentalsAsync(pagination, filter);

        // Assert
        result.Data.First().RentalId.Should().Be(expectedFirstId);
    }

    [Fact]
    public async Task GetAllRentalsAsync_WhenEmpty_ReturnsZeroTotals()
    {
        // Arrange
        var empty = new List<Rental>();
        _rentalRepositoryMock.Setup(r => r.GetAllRentals()).Returns(empty.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new RentalFilterDto();

        // Act
        var result = await _sut.GetAllRentalsAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    // =========================================================
    // GetRentalByIdAsync Tests
    // =========================================================

    [Fact]
    public async Task GetRentalByIdAsync_WhenExistsAndAuthorized_ReturnsDetailDto()
    {
        // Arrange
        var rental = CreateSampleRentals().First(r => r.RentalId == 1);
        _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(1)).ReturnsAsync(rental);
        _currentUserMock.Setup(u => u.IsCustomer).Returns(false);
        _currentUserMock.Setup(u => u.IsStaff).Returns(false);

        // Act
        var result = await _sut.GetRentalByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.RentalId.Should().Be(1);
        result.CustomerName.Should().Be("John Doe");
        result.TotalPaid.Should().Be(11.96m);
    }

    [Fact]
    public async Task GetRentalByIdAsync_WhenCustomerAttemptsToViewOtherCustomerRental_ReturnsNull()
    {
        // Arrange
        var rental = CreateSampleRentals().First(r => r.RentalId == 1); // CustomerId is 1
        _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(1)).ReturnsAsync(rental);
        _currentUserMock.Setup(u => u.IsCustomer).Returns(true);
        _currentUserMock.Setup(u => u.CustomerId).Returns(999); // Different customer

        // Act
        var result = await _sut.GetRentalByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRentalByIdAsync_WhenStaffAttemptsToViewOtherStoreRental_ReturnsNull()
    {
        // Arrange
        var rental = CreateSampleRentals().First(r => r.RentalId == 1); // Staff StoreId is 1
        _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(1)).ReturnsAsync(rental);
        _currentUserMock.Setup(u => u.IsCustomer).Returns(false);
        _currentUserMock.Setup(u => u.IsStaff).Returns(true);
        _currentUserMock.Setup(u => u.StoreId).Returns(2); // Different store

        // Act
        var result = await _sut.GetRentalByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRentalByIdAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(999)).ReturnsAsync((Rental?)null);

        // Act
        var result = await _sut.GetRentalByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================
    // CreateRentalAsync Tests
    // =========================================================

    [Fact]
    public async Task CreateRentalAsync_WhenValid_CreatesAndReturnsDto()
    {
        // Arrange
        var dto = new CreateRentalDto { InventoryId = 1, CustomerId = 1, StaffId = 1 };
        var movie = new Movie { MovieId = 1, Title = "Inception", RentalRate = 2.99m };
        var inventory = new Inventory { InventoryId = 1, MovieId = 1, Movie = movie, StoreId = 1 };

        _inventoryRepositoryMock.Setup(r => r.GetInventoryByIdAsync(1)).ReturnsAsync(inventory);
        _rentalRepositoryMock.Setup(r => r.CustomerExistsAsync(1)).ReturnsAsync(true);
        _rentalRepositoryMock.Setup(r => r.StaffExistsAsync(1)).ReturnsAsync(true);
        _rentalRepositoryMock.Setup(r => r.GetAllRentals()).Returns(new List<Rental>().BuildMock());

        _currentUserMock.Setup(u => u.IsStaff).Returns(false);

        var created = new Rental
        {
            RentalId = 100,
            InventoryId = 1,
            Inventory = inventory,
            CustomerId = 1,
            Customer = new Customer { CustomerId = 1, User = new User { FirstName = "John", LastName = "Doe" } },
            StaffId = 1,
            Staff = new Staff { StaffId = 1, User = new User { FirstName = "Mike", LastName = "Ross" } },
            RentalDate = DateTime.UtcNow
        };
        _rentalRepositoryMock.Setup(r => r.CreateRentalAsync(It.IsAny<Rental>())).ReturnsAsync(created);

        // Act
        var result = await _sut.CreateRentalAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.RentalId.Should().Be(100);
        result.MovieTitle.Should().Be("Inception");
        result.RentalRate.Should().Be(2.99m);
    }

    [Fact]
    public async Task CreateRentalAsync_WhenInventoryNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateRentalDto { InventoryId = 999, CustomerId = 1, StaffId = 1 };
        _inventoryRepositoryMock.Setup(r => r.GetInventoryByIdAsync(999)).ReturnsAsync((Inventory?)null);

        // Act
        var act = () => _sut.CreateRentalAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Inventory item #999 does not exist*");
    }

    [Fact]
    public async Task CreateRentalAsync_WhenMovieIsDeleted_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateRentalDto { InventoryId = 1, CustomerId = 1, StaffId = 1 };
        var deletedMovie = new Movie { MovieId = 1, Title = "Deleted Movie", IsDeleted = true };
        var inventory = new Inventory { InventoryId = 1, MovieId = 1, Movie = deletedMovie };
        _inventoryRepositoryMock.Setup(r => r.GetInventoryByIdAsync(1)).ReturnsAsync(inventory);

        // Act
        var act = () => _sut.CreateRentalAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*movie for inventory #1 does not exist or has been deleted*");
    }

    [Fact]
    public async Task CreateRentalAsync_WhenCustomerDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateRentalDto { InventoryId = 1, CustomerId = 999, StaffId = 1 };
        var inventory = new Inventory { InventoryId = 1, Movie = new Movie { MovieId = 1 } };
        _inventoryRepositoryMock.Setup(r => r.GetInventoryByIdAsync(1)).ReturnsAsync(inventory);
        _rentalRepositoryMock.Setup(r => r.CustomerExistsAsync(999)).ReturnsAsync(false);

        // Act
        var act = () => _sut.CreateRentalAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Customer #999 does not exist*");
    }

    [Fact]
    public async Task CreateRentalAsync_WhenInventoryCurrentlyRented_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateRentalDto { InventoryId = 1, CustomerId = 1, StaffId = 1 };
        var inventory = new Inventory { InventoryId = 1, Movie = new Movie { MovieId = 1 } };
        _inventoryRepositoryMock.Setup(r => r.GetInventoryByIdAsync(1)).ReturnsAsync(inventory);
        _rentalRepositoryMock.Setup(r => r.CustomerExistsAsync(1)).ReturnsAsync(true);
        _rentalRepositoryMock.Setup(r => r.StaffExistsAsync(1)).ReturnsAsync(true);

        var existingRentals = new List<Rental>
        {
            new() { RentalId = 5, InventoryId = 1, ReturnDate = null } // unreturned
        };
        _rentalRepositoryMock.Setup(r => r.GetAllRentals()).Returns(existingRentals.BuildMock());

        // Act
        var act = () => _sut.CreateRentalAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*currently rented out*");
    }

    [Fact]
    public async Task CreateRentalAsync_WhenStaffStoreDoesNotMatchInventoryStore_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateRentalDto { InventoryId = 1, CustomerId = 1, StaffId = 1 };
        var inventory = new Inventory { InventoryId = 1, StoreId = 2, Movie = new Movie { MovieId = 1 } };
        _inventoryRepositoryMock.Setup(r => r.GetInventoryByIdAsync(1)).ReturnsAsync(inventory);
        _rentalRepositoryMock.Setup(r => r.CustomerExistsAsync(1)).ReturnsAsync(true);
        _rentalRepositoryMock.Setup(r => r.StaffExistsAsync(1)).ReturnsAsync(true);
        _rentalRepositoryMock.Setup(r => r.GetAllRentals()).Returns(new List<Rental>().BuildMock());

        _currentUserMock.Setup(u => u.IsStaff).Returns(true);
        _currentUserMock.Setup(u => u.StoreId).Returns(1); // Staff assigned to Store 1, inventory is Store 2

        // Act
        var act = () => _sut.CreateRentalAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Staff can only create rentals for inventory belonging to their assigned store*");
    }

    // =========================================================
    // ReturnRentalAsync Tests
    // =========================================================

    [Fact]
    public async Task ReturnRentalAsync_WhenValid_ReturnsUpdatedDto()
    {
        // Arrange
        var rental = CreateSampleRentals().First(r => r.RentalId == 2); // Active (ReturnDate == null)
        _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(2)).ReturnsAsync(rental);
        _currentUserMock.Setup(u => u.IsStaff).Returns(false);

        var returned = new Rental
        {
            RentalId = 2,
            Inventory = rental.Inventory,
            Customer = rental.Customer,
            Staff = rental.Staff,
            RentalDate = rental.RentalDate,
            ReturnDate = DateTime.UtcNow
        };
        _rentalRepositoryMock.Setup(r => r.ReturnRentalAsync(2)).ReturnsAsync(returned);

        // Act
        var result = await _sut.ReturnRentalAsync(2);

        // Assert
        result.Should().NotBeNull();
        result!.RentalId.Should().Be(2);
        result.ReturnDate.Should().NotBeNull();
    }

    [Fact]
    public async Task ReturnRentalAsync_WhenAlreadyReturned_ThrowsInvalidOperationException()
    {
        // Arrange
        var rental = CreateSampleRentals().First(r => r.RentalId == 1); // Already returned (ReturnDate != null)
        _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(1)).ReturnsAsync(rental);

        // Act
        var act = () => _sut.ReturnRentalAsync(1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*has already been marked as returned*");
    }

    [Fact]
    public async Task ReturnRentalAsync_WhenStaffStoreMismatch_ThrowsInvalidOperationException()
    {
        // Arrange
        var rental = CreateSampleRentals().First(r => r.RentalId == 2); // Inventory StoreId = 2, Staff StoreId = 2
        _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(2)).ReturnsAsync(rental);
        _currentUserMock.Setup(u => u.IsStaff).Returns(true);
        _currentUserMock.Setup(u => u.StoreId).Returns(1); // Staff caller belongs to Store 1

        // Act
        var act = () => _sut.ReturnRentalAsync(2);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Staff can only process returns for rentals belonging to their assigned store*");
    }

    [Fact]
    public async Task ReturnRentalAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(999)).ReturnsAsync((Rental?)null);

        // Act
        var result = await _sut.ReturnRentalAsync(999);

        // Assert
        result.Should().BeNull();
    }
}
