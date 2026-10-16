using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Repositories;
using Xunit;

namespace MovieRental.Tests.Unit.Repositories;

public class StoreRepositoryTests
{
    [Fact]
    public async Task GetAllStores_ReturnsStoresWithManagerAndAddress()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var country = new Country { CountryId = 1, Name = "USA" };
        var city = new City { CityId = 1, Name = "Boston", CountryId = 1, Country = country };
        var address = new Address { AddressId = 1, Street = "100 Main St", CityId = 1, City = city };
        var managerUser = new User { UserId = 1, FirstName = "Manager", LastName = "User" };
        var managerStaff = new Staff { StaffId = 1, UserId = 1, User = managerUser };

        context.Countries.Add(country);
        context.Cities.Add(city);
        context.Addresses.Add(address);
        context.Users.Add(managerUser);
        context.Staff.Add(managerStaff);

        context.Stores.Add(new Store
        {
            StoreId = 1,
            ManagerStaffId = 1,
            ManagerStaff = managerStaff,
            AddressId = 1,
            Address = address
        });
        await context.SaveChangesAsync();

        var sut = new StoreRepository(context);

        // Act
        var stores = await sut.GetAllStores().ToListAsync();

        // Assert
        stores.Should().HaveCount(1);
        stores.First().ManagerStaff.Should().NotBeNull();
        stores.First().Address.Should().NotBeNull();
        stores.First().Address.City.Country.Name.Should().Be("USA");
    }

    [Fact]
    public async Task CreateStoreAsync_InsertsAndReloadsStore()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var country = new Country { CountryId = 1, Name = "USA" };
        var city = new City { CityId = 1, Name = "Austin", CountryId = 1, Country = country };
        var address = new Address { AddressId = 2, Street = "200 South St", CityId = 1, City = city };
        var managerUser = new User { UserId = 2, FirstName = "Alice", LastName = "Boss" };
        var managerStaff = new Staff { StaffId = 2, UserId = 2, User = managerUser };

        context.Countries.Add(country);
        context.Cities.Add(city);
        context.Addresses.Add(address);
        context.Users.Add(managerUser);
        context.Staff.Add(managerStaff);
        await context.SaveChangesAsync();

        var sut = new StoreRepository(context);
        var store = new Store { ManagerStaffId = 2, AddressId = 2 };

        // Act
        var created = await sut.CreateStoreAsync(store);

        // Assert
        created.StoreId.Should().BeGreaterThan(0);
        created.ManagerStaff.Should().NotBeNull();
        created.Address.Should().NotBeNull();
    }

    [Fact]
    public async Task AddressExistsAsync_ValidatesAddressPresence()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Addresses.Add(new Address { AddressId = 5, Street = "Active St", IsDeleted = false });
        context.Addresses.Add(new Address { AddressId = 6, Street = "Deleted St", IsDeleted = true });
        await context.SaveChangesAsync();

        var sut = new StoreRepository(context);

        // Act & Assert
        (await sut.AddressExistsAsync(5)).Should().BeTrue();
        (await sut.AddressExistsAsync(6)).Should().BeFalse();
        (await sut.AddressExistsAsync(999)).Should().BeFalse();
    }

    [Fact]
    public async Task StaffExistsAsync_ValidatesStaffPresence()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Staff.Add(new Staff { StaffId = 10, IsDeleted = false });
        context.Staff.Add(new Staff { StaffId = 20, IsDeleted = true });
        await context.SaveChangesAsync();

        var sut = new StoreRepository(context);

        // Act & Assert
        (await sut.StaffExistsAsync(10)).Should().BeTrue();
        (await sut.StaffExistsAsync(20)).Should().BeFalse();
        (await sut.StaffExistsAsync(999)).Should().BeFalse();
    }
}
