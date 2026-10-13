using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable;
using MockQueryable.EntityFrameworkCore;
using Moq;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Services;
using Xunit;

namespace MovieRental.Tests.Unit.Services;

public class DashboardServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMovieRepository> _movieRepositoryMock;
    private readonly Mock<IRentalRepository> _rentalRepositoryMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IStaffRepository> _staffRepositoryMock;

    private readonly DashboardService _sut;

    public DashboardServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _movieRepositoryMock = new Mock<IMovieRepository>();
        _rentalRepositoryMock = new Mock<IRentalRepository>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _staffRepositoryMock = new Mock<IStaffRepository>();

        _sut = new DashboardService(
            _userRepositoryMock.Object,
            _movieRepositoryMock.Object,
            _rentalRepositoryMock.Object,
            _paymentRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _staffRepositoryMock.Object);
    }

    private static (List<User> Users, List<Movie> Movies, List<Inventory> Inventories,
                    List<Customer> Customers, List<Staff> StaffList, List<Rental> Rentals, List<Payment> Payments)
        CreateSampleData()
    {
        var users = new List<User>
        {
            new() { UserId = 1, FirstName = "Admin", LastName = "User" },
            new() { UserId = 2, FirstName = "Staff", LastName = "One" },
            new() { UserId = 3, FirstName = "Customer", LastName = "One" },
            new() { UserId = 4, FirstName = "Customer", LastName = "Two" }
        };

        var staff1 = new Staff { StaffId = 1, UserId = 2, StoreId = 1, User = users[1] };
        var staff2 = new Staff { StaffId = 2, UserId = 2, StoreId = 2, User = users[1] };
        var staffList = new List<Staff> { staff1, staff2 };

        var cust1 = new Customer { CustomerId = 1, UserId = 3, StoreId = 1, User = users[2] };
        var cust2 = new Customer { CustomerId = 2, UserId = 4, StoreId = 2, User = users[3] };
        var customers = new List<Customer> { cust1, cust2 };

        var movie1 = new Movie { MovieId = 1, Title = "Inception", RentalRate = 2.99m };
        var movie2 = new Movie { MovieId = 2, Title = "The Dark Knight", RentalRate = 3.99m };
        var movies = new List<Movie> { movie1, movie2 };

        var inv1 = new Inventory { InventoryId = 1, MovieId = 1, Movie = movie1, StoreId = 1 };
        var inv2 = new Inventory { InventoryId = 2, MovieId = 2, Movie = movie2, StoreId = 1 };
        var inventories = new List<Inventory> { inv1, inv2 };

        movie1.Inventories = new List<Inventory> { inv1 };
        movie2.Inventories = new List<Inventory> { inv2 };

        var rental1 = new Rental
        {
            RentalId = 1,
            RentalDate = DateTime.UtcNow.AddDays(-2),
            ReturnDate = DateTime.UtcNow.AddDays(-1), // Returned
            InventoryId = 1,
            Inventory = inv1,
            CustomerId = 1,
            Customer = cust1,
            StaffId = 1,
            Staff = staff1
        };

        var rental2 = new Rental
        {
            RentalId = 2,
            RentalDate = DateTime.UtcNow,
            ReturnDate = null, // Active
            InventoryId = 2,
            Inventory = inv2,
            CustomerId = 1,
            Customer = cust1,
            StaffId = 1,
            Staff = staff1
        };

        var rentals = new List<Rental> { rental1, rental2 };
        inv1.Rentals = new List<Rental> { rental1 };
        inv2.Rentals = new List<Rental> { rental2 };

        var payment1 = new Payment
        {
            PaymentId = 1,
            RentalId = 1,
            Rental = rental1,
            CustomerId = 1,
            Customer = cust1,
            StaffId = 1,
            Staff = staff1,
            Amount = 10.00m,
            PaymentDate = DateTime.UtcNow.Date // Today
        };

        var payment2 = new Payment
        {
            PaymentId = 2,
            RentalId = 2,
            Rental = rental2,
            CustomerId = 2,
            Customer = cust2,
            StaffId = 2,
            Staff = staff2,
            Amount = 15.00m,
            PaymentDate = DateTime.UtcNow.Date.AddDays(-1) // Yesterday
        };

        var payments = new List<Payment> { payment1, payment2 };

        return (users, movies, inventories, customers, staffList, rentals, payments);
    }

    // =========================================================
    // GetAdminDashboardAsync Tests
    // =========================================================

    [Fact]
    public async Task GetAdminDashboardAsync_WhenDataExists_ReturnsAggregatedKpis()
    {
        // Arrange
        var (users, movies, inventories, customers, staffList, rentals, payments) = CreateSampleData();

        _userRepositoryMock.Setup(r => r.GetAllUsers()).Returns(users.BuildMock());
        _movieRepositoryMock.Setup(r => r.GetAllMovies()).Returns(movies.BuildMock());
        _inventoryRepositoryMock.Setup(r => r.GetAllInventory()).Returns(inventories.BuildMock());
        _customerRepositoryMock.Setup(r => r.GetAllCustomers()).Returns(customers.BuildMock());
        _staffRepositoryMock.Setup(r => r.GetAllStaff()).Returns(staffList.BuildMock());
        _rentalRepositoryMock.Setup(r => r.GetAllRentals()).Returns(rentals.BuildMock());
        _paymentRepositoryMock.Setup(r => r.GetAllPayments()).Returns(payments.BuildMock());

        // Act
        var result = await _sut.GetAdminDashboardAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalUsers.Should().Be(4);
        result.TotalMovies.Should().Be(2);
        result.TotalRentals.Should().Be(2);
        result.TotalRevenue.Should().Be(25.00m);
        result.ActiveRentals.Should().Be(1); // rental2
        result.AvailableInventory.Should().Be(1); // inv1 (inv2 is rented out)
        result.TotalCustomers.Should().Be(2);
        result.TotalStaff.Should().Be(2);

        result.TopRentedMovies.Should().NotBeEmpty();
        result.RevenueByStore.Should().HaveCount(2);
        result.RevenueByStore.Should().Contain(s => s.StoreId == 1 && s.Revenue == 10.00m);
        result.RevenueByStore.Should().Contain(s => s.StoreId == 2 && s.Revenue == 15.00m);
        result.RecentRentals.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAdminDashboardAsync_WhenEmpty_ReturnsZeroValues()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetAllUsers()).Returns(new List<User>().BuildMock());
        _movieRepositoryMock.Setup(r => r.GetAllMovies()).Returns(new List<Movie>().BuildMock());
        _inventoryRepositoryMock.Setup(r => r.GetAllInventory()).Returns(new List<Inventory>().BuildMock());
        _customerRepositoryMock.Setup(r => r.GetAllCustomers()).Returns(new List<Customer>().BuildMock());
        _staffRepositoryMock.Setup(r => r.GetAllStaff()).Returns(new List<Staff>().BuildMock());
        _rentalRepositoryMock.Setup(r => r.GetAllRentals()).Returns(new List<Rental>().BuildMock());
        _paymentRepositoryMock.Setup(r => r.GetAllPayments()).Returns(new List<Payment>().BuildMock());

        // Act
        var result = await _sut.GetAdminDashboardAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalUsers.Should().Be(0);
        result.TotalMovies.Should().Be(0);
        result.TotalRentals.Should().Be(0);
        result.TotalRevenue.Should().Be(0m);
        result.ActiveRentals.Should().Be(0);
        result.AvailableInventory.Should().Be(0);
        result.TopRentedMovies.Should().BeEmpty();
        result.RevenueByStore.Should().BeEmpty();
        result.RecentRentals.Should().BeEmpty();
    }

    // =========================================================
    // GetStaffDashboardAsync Tests
    // =========================================================

    [Fact]
    public async Task GetStaffDashboardAsync_WhenStaffExists_ReturnsStoreScopedMetrics()
    {
        // Arrange
        var (_, _, inventories, customers, staffList, rentals, payments) = CreateSampleData();

        _staffRepositoryMock.Setup(r => r.GetAllStaff()).Returns(staffList.BuildMock());
        _inventoryRepositoryMock.Setup(r => r.GetAllInventory()).Returns(inventories.BuildMock());
        _customerRepositoryMock.Setup(r => r.GetAllCustomers()).Returns(customers.BuildMock());
        _rentalRepositoryMock.Setup(r => r.GetAllRentals()).Returns(rentals.BuildMock());
        _paymentRepositoryMock.Setup(r => r.GetAllPayments()).Returns(payments.BuildMock());

        // Act - Staff with UserId 2 is assigned to Store 1
        var result = await _sut.GetStaffDashboardAsync(2);

        // Assert
        result.Should().NotBeNull();
        result.StoreId.Should().Be(1);
        result.ActiveRentals.Should().Be(1); // rental2 is in Store 1
        result.AvailableInventory.Should().Be(1); // inv1 is in Store 1
        result.TotalCustomers.Should().Be(1); // cust1 is in Store 1
        result.TodaysPayments.Should().Be(10.00m); // payment1 was made today in Store 1
        result.RecentRentals.Should().HaveCount(2);
        result.RecentPayments.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetStaffDashboardAsync_WhenStaffNotFound_ReturnsZeroMetricsForStoreZero()
    {
        // Arrange
        var emptyStaff = new List<Staff>();
        _staffRepositoryMock.Setup(r => r.GetAllStaff()).Returns(emptyStaff.BuildMock());
        _inventoryRepositoryMock.Setup(r => r.GetAllInventory()).Returns(new List<Inventory>().BuildMock());
        _customerRepositoryMock.Setup(r => r.GetAllCustomers()).Returns(new List<Customer>().BuildMock());
        _rentalRepositoryMock.Setup(r => r.GetAllRentals()).Returns(new List<Rental>().BuildMock());
        _paymentRepositoryMock.Setup(r => r.GetAllPayments()).Returns(new List<Payment>().BuildMock());

        // Act
        var result = await _sut.GetStaffDashboardAsync(999);

        // Assert
        result.Should().NotBeNull();
        result.StoreId.Should().Be(0);
        result.ActiveRentals.Should().Be(0);
        result.AvailableInventory.Should().Be(0);
        result.TodaysPayments.Should().Be(0m);
    }

    // =========================================================
    // GetCustomerDashboardAsync Tests
    // =========================================================

    [Fact]
    public async Task GetCustomerDashboardAsync_WhenCustomerExists_ReturnsCustomerMetrics()
    {
        // Arrange
        var (_, _, _, customers, _, rentals, payments) = CreateSampleData();

        _customerRepositoryMock.Setup(r => r.GetAllCustomers()).Returns(customers.BuildMock());
        _rentalRepositoryMock.Setup(r => r.GetAllRentals()).Returns(rentals.BuildMock());
        _paymentRepositoryMock.Setup(r => r.GetAllPayments()).Returns(payments.BuildMock());

        // Act - Customer with UserId 3 has CustomerId 1
        var result = await _sut.GetCustomerDashboardAsync(3);

        // Assert
        result.Should().NotBeNull();
        result.ActiveRentals.Should().Be(1); // rental2 has ReturnDate == null
        result.TotalRentals.Should().Be(2);  // rental1 and rental2
        result.TotalSpent.Should().Be(10.00m); // payment1
        result.MyActiveRentals.Should().HaveCount(1);
        result.MyActiveRentals.First().RentalId.Should().Be(2);
    }

    [Fact]
    public async Task GetCustomerDashboardAsync_WhenCustomerNotFound_ReturnsZeroMetrics()
    {
        // Arrange
        var emptyCustomers = new List<Customer>();
        _customerRepositoryMock.Setup(r => r.GetAllCustomers()).Returns(emptyCustomers.BuildMock());
        _rentalRepositoryMock.Setup(r => r.GetAllRentals()).Returns(new List<Rental>().BuildMock());
        _paymentRepositoryMock.Setup(r => r.GetAllPayments()).Returns(new List<Payment>().BuildMock());

        // Act
        var result = await _sut.GetCustomerDashboardAsync(999);

        // Assert
        result.Should().NotBeNull();
        result.ActiveRentals.Should().Be(0);
        result.TotalRentals.Should().Be(0);
        result.TotalSpent.Should().Be(0m);
        result.MyActiveRentals.Should().BeEmpty();
    }
}
