using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Repositories;
using Xunit;

namespace MovieRental.Tests.Unit.Repositories;

public class AddressRepositoryTests
{
    [Fact]
    public async Task GetAllAddresses_ReturnsAddressesWithCityAndCountry()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var country = new Country { CountryId = 1, Name = "USA" };
        var city = new City { CityId = 1, Name = "Seattle", CountryId = 1, Country = country };
        context.Countries.Add(country);
        context.Cities.Add(city);
        context.Addresses.AddRange(
            new Address { AddressId = 1, Street = "1st Ave", CityId = 1, City = city },
            new Address { AddressId = 2, Street = "2nd Ave", CityId = 1, City = city }
        );
        await context.SaveChangesAsync();

        var sut = new AddressRepository(context);

        // Act
        var addresses = await sut.GetAllAddresses().ToListAsync();

        // Assert
        addresses.Should().HaveCount(2);
        addresses.First().City.Should().NotBeNull();
        addresses.First().City.Country.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAddressByIdAsync_WhenExists_ReturnsAddressWithRelations()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var country = new Country { CountryId = 1, Name = "USA" };
        var city = new City { CityId = 1, Name = "Chicago", CountryId = 1, Country = country };
        var address = new Address
        {
            AddressId = 10,
            Street = "Michigan Ave",
            PostalCode = "60601",
            Phone = "1234567890",
            CityId = 1,
            City = city,
            Users = new List<User> { new() { UserId = 1, FirstName = "Alice" } },
            Stores = new List<Store> { new() { StoreId = 1 } }
        };
        context.Countries.Add(country);
        context.Cities.Add(city);
        context.Addresses.Add(address);
        await context.SaveChangesAsync();

        var sut = new AddressRepository(context);

        // Act
        var result = await sut.GetAddressByIdAsync(10);

        // Assert
        result.Should().NotBeNull();
        result!.Street.Should().Be("Michigan Ave");
        result.Users.Should().HaveCount(1);
        result.Stores.Should().HaveCount(1);
        result.City.Country.Name.Should().Be("USA");
    }

    [Fact]
    public async Task CreateAddressAsync_InsertsAndReloadsAddress()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var country = new Country { CountryId = 1, Name = "UK" };
        var city = new City { CityId = 1, Name = "London", CountryId = 1, Country = country };
        context.Countries.Add(country);
        context.Cities.Add(city);
        await context.SaveChangesAsync();

        var sut = new AddressRepository(context);
        var newAddress = new Address { Street = "Baker St 221B", PostalCode = "NW1 6XE", Phone = "02079460999", CityId = 1 };

        // Act
        var created = await sut.CreateAddressAsync(newAddress);

        // Assert
        created.AddressId.Should().BeGreaterThan(0);
        created.Street.Should().Be("Baker St 221B");
        created.City.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAddressAsync_WhenExists_UpdatesFields()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var country = new Country { CountryId = 1, Name = "USA" };
        var city = new City { CityId = 1, Name = "Dallas", CountryId = 1, Country = country };
        context.Countries.Add(country);
        context.Cities.Add(city);
        context.Addresses.Add(new Address { AddressId = 5, Street = "Old Street", Phone = "111", CityId = 1, City = city });
        await context.SaveChangesAsync();

        var sut = new AddressRepository(context);
        var updateModel = new Address { AddressId = 5, Street = "New Street", Phone = "999", CityId = 1 };

        // Act
        var updated = await sut.UpdateAddressAsync(updateModel);

        // Assert
        updated.Should().NotBeNull();
        updated!.Street.Should().Be("New Street");
        updated.Phone.Should().Be("999");
    }

    [Fact]
    public async Task DeleteAddressAsync_WhenExists_SoftDeletesAddress()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Addresses.Add(new Address { AddressId = 1, Street = "Active St", IsDeleted = false });
        await context.SaveChangesAsync();

        var sut = new AddressRepository(context);

        // Act
        var success = await sut.DeleteAddressAsync(1);

        // Assert
        success.Should().BeTrue();
        var inDb = await context.Addresses.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.AddressId == 1);
        inDb!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task CityExistsAsync_ChecksPresenceInDatabase()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Cities.Add(new City { CityId = 10, Name = "ActiveCity", IsDeleted = false });
        context.Cities.Add(new City { CityId = 20, Name = "DeletedCity", IsDeleted = true });
        await context.SaveChangesAsync();

        var sut = new AddressRepository(context);

        // Act & Assert
        (await sut.CityExistsAsync(10)).Should().BeTrue();
        (await sut.CityExistsAsync(20)).Should().BeFalse();
        (await sut.CityExistsAsync(999)).Should().BeFalse();
    }
}
