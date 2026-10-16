using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Repositories;
using Xunit;

namespace MovieRental.Tests.Unit.Repositories;

public class CustomerRepositoryTests
{
    [Fact]
    public async Task GetAllCustomers_ReturnsCustomersWithUserAccount()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var user = new User { UserId = 1, FirstName = "Jane", LastName = "Customer" };
        context.Users.Add(user);
        context.Customers.Add(new Customer
        {
            CustomerId = 1,
            UserId = 1,
            User = user,
            StoreId = 1,
            CreateDate = new DateOnly(2025, 1, 1)
        });
        await context.SaveChangesAsync();

        var sut = new CustomerRepository(context);

        // Act
        var customers = await sut.GetAllCustomers().ToListAsync();

        // Assert
        customers.Should().HaveCount(1);
        customers.First().User.Should().NotBeNull();
        customers.First().User!.LastName.Should().Be("Customer");
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WhenExists_ReturnsCustomerWithHierarchy()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var role = new Role { RoleId = 2, RoleName = "Customer" };
        var country = new Country { CountryId = 1, Name = "Canada" };
        var city = new City { CityId = 1, Name = "Montreal", CountryId = 1, Country = country };
        var address = new Address { AddressId = 1, Street = "Saint Catherine St", CityId = 1, City = city };
        var user = new User
        {
            UserId = 10,
            FirstName = "Pierre",
            LastName = "Dubois",
            RoleId = 2,
            Role = role,
            AddressId = 1,
            Address = address
        };
        var customer = new Customer
        {
            CustomerId = 7,
            UserId = 10,
            User = user,
            StoreId = 1,
            CreateDate = new DateOnly(2025, 2, 1)
        };

        context.Roles.Add(role);
        context.Countries.Add(country);
        context.Cities.Add(city);
        context.Addresses.Add(address);
        context.Users.Add(user);
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var sut = new CustomerRepository(context);

        // Act
        var result = await sut.GetCustomerByIdAsync(7);

        // Assert
        result.Should().NotBeNull();
        result!.CustomerId.Should().Be(7);
        result.User.Should().NotBeNull();
        result.User!.Role!.RoleName.Should().Be("Customer");
        result.User.Address!.City!.Country!.Name.Should().Be("Canada");
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var sut = new CustomerRepository(context);

        // Act
        var result = await sut.GetCustomerByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }
}
