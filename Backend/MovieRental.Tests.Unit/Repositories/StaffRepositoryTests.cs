using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Repositories;
using Xunit;

namespace MovieRental.Tests.Unit.Repositories;

public class StaffRepositoryTests
{
    [Fact]
    public async Task GetAllStaff_ReturnsStaffWithUserAccount()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var user = new User { UserId = 1, FirstName = "Sarah", LastName = "Connor" };
        context.Users.Add(user);
        context.Staff.Add(new Staff { StaffId = 1, UserId = 1, User = user, StoreId = 1 });
        await context.SaveChangesAsync();

        var sut = new StaffRepository(context);

        // Act
        var staffList = await sut.GetAllStaff().ToListAsync();

        // Assert
        staffList.Should().HaveCount(1);
        staffList.First().User.Should().NotBeNull();
        staffList.First().User!.FirstName.Should().Be("Sarah");
    }

    [Fact]
    public async Task GetStaffByIdAsync_WhenExists_ReturnsStaffWithHierarchy()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var role = new Role { RoleId = 1, RoleName = "Staff" };
        var country = new Country { CountryId = 1, Name = "USA" };
        var city = new City { CityId = 1, Name = "Denver", CountryId = 1, Country = country };
        var address = new Address { AddressId = 1, Street = "Colfax Ave", CityId = 1, City = city };
        var user = new User
        {
            UserId = 10,
            FirstName = "John",
            LastName = "Doe",
            RoleId = 1,
            Role = role,
            AddressId = 1,
            Address = address
        };
        var staff = new Staff { StaffId = 5, UserId = 10, User = user, StoreId = 1 };

        context.Roles.Add(role);
        context.Countries.Add(country);
        context.Cities.Add(city);
        context.Addresses.Add(address);
        context.Users.Add(user);
        context.Staff.Add(staff);
        await context.SaveChangesAsync();

        var sut = new StaffRepository(context);

        // Act
        var result = await sut.GetStaffByIdAsync(5);

        // Assert
        result.Should().NotBeNull();
        result!.User.Should().NotBeNull();
        result.User!.Role.Should().NotBeNull();
        result.User.Role!.RoleName.Should().Be("Staff");
        result.User.Address!.City!.Country!.Name.Should().Be("USA");
    }

    [Fact]
    public async Task GetStaffByIdAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var sut = new StaffRepository(context);

        // Act
        var result = await sut.GetStaffByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }
}
