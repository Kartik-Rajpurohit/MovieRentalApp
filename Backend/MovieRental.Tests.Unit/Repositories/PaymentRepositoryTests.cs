using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Repositories;
using Xunit;

namespace MovieRental.Tests.Unit.Repositories;

public class PaymentRepositoryTests
{
    [Fact]
    public async Task GetAllPayments_ReturnsPaymentsWithoutTracking()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Payments.AddRange(
            new Payment { PaymentId = 1, CustomerId = 1, StaffId = 1, RentalId = 1, Amount = 4.99m, PaymentDate = DateTime.UtcNow },
            new Payment { PaymentId = 2, CustomerId = 2, StaffId = 1, RentalId = 2, Amount = 7.99m, PaymentDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var sut = new PaymentRepository(context);

        // Act
        var payments = await sut.GetAllPayments().ToListAsync();

        // Assert
        payments.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPaymentByIdAsync_WhenExists_ReturnsPaymentWithRelations()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var movie = new Movie { MovieId = 1, Title = "Interstellar" };
        var inventory = new Inventory { InventoryId = 1, MovieId = 1, Movie = movie, StoreId = 1 };
        var rental = new Rental { RentalId = 1, Inventory = inventory };

        var custUser = new User { UserId = 1, FirstName = "Alice" };
        var customer = new Customer { CustomerId = 1, UserId = 1, User = custUser };

        var staffUser = new User { UserId = 2, FirstName = "Bob" };
        var staff = new Staff { StaffId = 1, UserId = 2, User = staffUser };

        var payment = new Payment
        {
            PaymentId = 10,
            RentalId = 1,
            Rental = rental,
            CustomerId = 1,
            Customer = customer,
            StaffId = 1,
            Staff = staff,
            Amount = 14.99m,
            PaymentDate = DateTime.UtcNow
        };

        context.Movies.Add(movie);
        context.Inventories.Add(inventory);
        context.Rentals.Add(rental);
        context.Users.AddRange(custUser, staffUser);
        context.Customers.Add(customer);
        context.Staff.Add(staff);
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var sut = new PaymentRepository(context);

        // Act
        var result = await sut.GetPaymentByIdAsync(10);

        // Assert
        result.Should().NotBeNull();
        result!.Amount.Should().Be(14.99m);
        result.Customer.User!.FirstName.Should().Be("Alice");
        result.Staff.User!.FirstName.Should().Be("Bob");
        result.Rental.Inventory.Movie.Title.Should().Be("Interstellar");
    }

    [Fact]
    public async Task CreatePaymentAsync_InsertsAndReloadsPayment()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var movie = new Movie { MovieId = 1, Title = "Dune" };
        var inventory = new Inventory { InventoryId = 1, MovieId = 1, Movie = movie, StoreId = 1 };
        var rental = new Rental { RentalId = 1, Inventory = inventory };
        var user = new User { UserId = 1, FirstName = "Paul" };
        var customer = new Customer { CustomerId = 1, UserId = 1, User = user };
        var staff = new Staff { StaffId = 1, UserId = 1, User = user };

        context.Movies.Add(movie);
        context.Inventories.Add(inventory);
        context.Rentals.Add(rental);
        context.Users.Add(user);
        context.Customers.Add(customer);
        context.Staff.Add(staff);
        await context.SaveChangesAsync();

        var sut = new PaymentRepository(context);
        var payment = new Payment
        {
            CustomerId = 1,
            StaffId = 1,
            RentalId = 1,
            Amount = 19.99m,
            PaymentDate = DateTime.UtcNow
        };

        // Act
        var created = await sut.CreatePaymentAsync(payment);

        // Assert
        created.PaymentId.Should().BeGreaterThan(0);
        created.Customer.Should().NotBeNull();
        created.Rental.Should().NotBeNull();
    }
}
