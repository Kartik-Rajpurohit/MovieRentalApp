using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Repositories;
using Xunit;

namespace MovieRental.Tests.Unit.Repositories;

public class RentalRepositoryTests
{
    [Fact]
    public async Task GetAllRentals_ReturnsRentalsWithoutTracking()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Rentals.AddRange(
            new Rental { RentalId = 1, RentalDate = DateTime.UtcNow, InventoryId = 1, CustomerId = 1, StaffId = 1 },
            new Rental { RentalId = 2, RentalDate = DateTime.UtcNow, InventoryId = 2, CustomerId = 2, StaffId = 1 }
        );
        await context.SaveChangesAsync();

        var sut = new RentalRepository(context);

        // Act
        var rentals = await sut.GetAllRentals().ToListAsync();

        // Assert
        rentals.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRentalByIdAsync_WhenExists_ReturnsRentalWithFullHierarchy()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var movie = new Movie { MovieId = 1, Title = "Inception" };
        var inventory = new Inventory { InventoryId = 1, MovieId = 1, Movie = movie, StoreId = 1 };
        var custUser = new User { UserId = 1, FirstName = "John", LastName = "Customer" };
        var customer = new Customer { CustomerId = 1, UserId = 1, User = custUser };
        var staffUser = new User { UserId = 2, FirstName = "Sam", LastName = "Staff" };
        var staff = new Staff { StaffId = 1, UserId = 2, User = staffUser };

        var rental = new Rental
        {
            RentalId = 10,
            RentalDate = DateTime.UtcNow,
            InventoryId = 1,
            Inventory = inventory,
            CustomerId = 1,
            Customer = customer,
            StaffId = 1,
            Staff = staff,
            Payments = new List<Payment> { new() { PaymentId = 100, Amount = 9.99m } }
        };

        context.Movies.Add(movie);
        context.Inventories.Add(inventory);
        context.Users.AddRange(custUser, staffUser);
        context.Customers.Add(customer);
        context.Staff.Add(staff);
        context.Rentals.Add(rental);
        await context.SaveChangesAsync();

        var sut = new RentalRepository(context);

        // Act
        var result = await sut.GetRentalByIdAsync(10);

        // Assert
        result.Should().NotBeNull();
        result!.Inventory.Movie.Title.Should().Be("Inception");
        result.Customer.User!.FirstName.Should().Be("John");
        result.Staff.User!.FirstName.Should().Be("Sam");
        result.Payments.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateRentalAsync_InsertsAndReloadsRental()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var movie = new Movie { MovieId = 1, Title = "Gladiator" };
        var inventory = new Inventory { InventoryId = 1, MovieId = 1, Movie = movie, StoreId = 1 };
        var user = new User { UserId = 1, FirstName = "Alice" };
        var customer = new Customer { CustomerId = 1, UserId = 1, User = user };
        var staff = new Staff { StaffId = 1, UserId = 1, User = user };

        context.Movies.Add(movie);
        context.Inventories.Add(inventory);
        context.Users.Add(user);
        context.Customers.Add(customer);
        context.Staff.Add(staff);
        await context.SaveChangesAsync();

        var sut = new RentalRepository(context);
        var rental = new Rental
        {
            RentalDate = DateTime.UtcNow,
            InventoryId = 1,
            CustomerId = 1,
            StaffId = 1
        };

        // Act
        var created = await sut.CreateRentalAsync(rental);

        // Assert
        created.RentalId.Should().BeGreaterThan(0);
        created.Inventory.Should().NotBeNull();
    }

    [Fact]
    public async Task ReturnRentalAsync_SetsReturnDateWhenActive()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var movie = new Movie { MovieId = 1, Title = "Movie" };
        var inventory = new Inventory { InventoryId = 1, MovieId = 1, Movie = movie, StoreId = 1 };
        var user = new User { UserId = 1, FirstName = "User" };
        var customer = new Customer { CustomerId = 1, UserId = 1, User = user };
        var staff = new Staff { StaffId = 1, UserId = 1, User = user };

        context.Movies.Add(movie);
        context.Inventories.Add(inventory);
        context.Users.Add(user);
        context.Customers.Add(customer);
        context.Staff.Add(staff);

        var rental = new Rental
        {
            RentalId = 5,
            RentalDate = DateTime.UtcNow.AddDays(-3),
            ReturnDate = null,
            InventoryId = 1,
            CustomerId = 1,
            StaffId = 1
        };
        context.Rentals.Add(rental);
        await context.SaveChangesAsync();

        var sut = new RentalRepository(context);

        // Act
        var returned = await sut.ReturnRentalAsync(5);

        // Assert
        returned.Should().NotBeNull();
        returned!.ReturnDate.Should().NotBeNull();
    }

    [Fact]
    public async Task ReturnRentalAsync_WhenAlreadyReturned_DoesNotOverwriteReturnDate()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var movie = new Movie { MovieId = 1, Title = "Movie" };
        var inventory = new Inventory { InventoryId = 1, MovieId = 1, Movie = movie, StoreId = 1 };
        var user = new User { UserId = 1, FirstName = "User" };
        var customer = new Customer { CustomerId = 1, UserId = 1, User = user };
        var staff = new Staff { StaffId = 1, UserId = 1, User = user };

        context.Movies.Add(movie);
        context.Inventories.Add(inventory);
        context.Users.Add(user);
        context.Customers.Add(customer);
        context.Staff.Add(staff);

        var initialReturnDate = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var rental = new Rental
        {
            RentalId = 6,
            RentalDate = DateTime.UtcNow.AddDays(-10),
            ReturnDate = initialReturnDate,
            InventoryId = 1,
            CustomerId = 1,
            StaffId = 1
        };
        context.Rentals.Add(rental);
        await context.SaveChangesAsync();

        var sut = new RentalRepository(context);

        // Act
        var result = await sut.ReturnRentalAsync(6, DateTime.UtcNow);

        // Assert
        result.Should().NotBeNull();
        result!.ReturnDate.Should().Be(initialReturnDate);
    }

    [Fact]
    public async Task CustomerExistsAsync_StaffExistsAsync_ValidatesPresence()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Customers.Add(new Customer { CustomerId = 1, IsDeleted = false });
        context.Staff.Add(new Staff { StaffId = 1, IsDeleted = false });
        await context.SaveChangesAsync();

        var sut = new RentalRepository(context);

        // Act & Assert
        (await sut.CustomerExistsAsync(1)).Should().BeTrue();
        (await sut.CustomerExistsAsync(999)).Should().BeFalse();
        (await sut.StaffExistsAsync(1)).Should().BeTrue();
        (await sut.StaffExistsAsync(999)).Should().BeFalse();
    }
}
