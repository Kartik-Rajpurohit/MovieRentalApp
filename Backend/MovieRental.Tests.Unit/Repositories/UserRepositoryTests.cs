using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Repositories;
using Xunit;

namespace MovieRental.Tests.Unit.Repositories;

public class UserRepositoryTests
{
    [Fact]
    public async Task GetAllUsers_ReturnsUsersWithRoleAndAddress()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var role = new Role { RoleId = 1, RoleName = "Customer" };
        var country = new Country { CountryId = 1, Name = "USA" };
        var city = new City { CityId = 1, Name = "Austin", CountryId = 1, Country = country };
        var address = new Address { AddressId = 1, Street = "Congress Ave", CityId = 1, City = city };

        context.Roles.Add(role);
        context.Countries.Add(country);
        context.Cities.Add(city);
        context.Addresses.Add(address);
        context.Users.Add(new User
        {
            UserId = 1,
            FirstName = "Bruce",
            LastName = "Wayne",
            Email = "bruce@wayne.com",
            RoleId = 1,
            Role = role,
            AddressId = 1,
            Address = address
        });
        await context.SaveChangesAsync();

        var sut = new UserRepository(context);

        // Act
        var users = await sut.GetAllUsers().ToListAsync();

        // Assert
        users.Should().HaveCount(1);
        users.First().Role!.RoleName.Should().Be("Customer");
        users.First().Address!.City!.Country!.Name.Should().Be("USA");
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenExists_ReturnsUserWithRelations()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var role = new Role { RoleId = 2, RoleName = "Staff" };
        var user = new User
        {
            UserId = 5,
            FirstName = "Clark",
            LastName = "Kent",
            Email = "clark@dailyplanet.com",
            RoleId = 2,
            Role = role,
            Staff = new Staff { StaffId = 3, StoreId = 1 }
        };
        context.Roles.Add(role);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var sut = new UserRepository(context);

        // Act
        var result = await sut.GetUserByIdAsync(5);

        // Assert
        result.Should().NotBeNull();
        result!.Staff.Should().NotBeNull();
        result.Staff!.StoreId.Should().Be(1);
        result.Role!.RoleName.Should().Be("Staff");
    }

    [Fact]
    public async Task EmailExistsAsync_ChecksPresenceOfEmail()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Users.Add(new User { UserId = 1, Email = "test@example.com" });
        await context.SaveChangesAsync();

        var sut = new UserRepository(context);

        // Act & Assert
        (await sut.EmailExistsAsync("test@example.com")).Should().BeTrue();
        (await sut.EmailExistsAsync("other@example.com")).Should().BeFalse();
    }

    [Fact]
    public async Task CreateUserAsync_InsertsUserAndExplicitlyLoadsRelations()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var role = new Role { RoleId = 1, RoleName = "Admin" };
        var country = new Country { CountryId = 1, Name = "USA" };
        var city = new City { CityId = 1, Name = "Dallas", CountryId = 1, Country = country };
        var address = new Address { AddressId = 1, Street = "Main St", CityId = 1, City = city };

        context.Roles.Add(role);
        context.Countries.Add(country);
        context.Cities.Add(city);
        context.Addresses.Add(address);
        await context.SaveChangesAsync();

        var sut = new UserRepository(context);
        var newUser = new User
        {
            FirstName = "Barry",
            LastName = "Allen",
            Email = "flash@star.labs",
            PasswordHash = "hash123",
            RoleId = 1,
            AddressId = 1
        };

        // Act
        var created = await sut.CreateUserAsync(newUser);

        // Assert
        created.UserId.Should().BeGreaterThan(0);
        created.Role.Should().NotBeNull();
        created.Role!.RoleName.Should().Be("Admin");
        created.Address!.City!.Country!.Name.Should().Be("USA");
    }

    [Fact]
    public async Task ToggleUserStatusAsync_FlipsIsActiveStatus()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var user = new User { UserId = 1, FirstName = "Diana", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var sut = new UserRepository(context);

        // Act & Assert - Turn off
        var toggledOff = await sut.ToggleUserStatusAsync(1);
        toggledOff!.IsActive.Should().BeFalse();

        // Turn back on
        var toggledOn = await sut.ToggleUserStatusAsync(1);
        toggledOn!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateStaffAsync_And_UpdateStaffStoreAsync_ManageStaffRecord()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var user = new User { UserId = 10, FirstName = "StaffMember" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var sut = new UserRepository(context);

        // Act 1 - Create
        await sut.CreateStaffAsync(10, 1);
        var staff = await context.Staff.FirstOrDefaultAsync(s => s.UserId == 10);
        staff.Should().NotBeNull();
        staff!.StoreId.Should().Be(1);

        // Act 2 - Update existing store
        await sut.UpdateStaffStoreAsync(10, 2);
        var updated = await context.Staff.FirstOrDefaultAsync(s => s.UserId == 10);
        updated!.StoreId.Should().Be(2);

        // Act 3 - Update non-existent staff creates record
        await sut.UpdateStaffStoreAsync(20, 5);
        var createdOnFly = await context.Staff.FirstOrDefaultAsync(s => s.UserId == 20);
        createdOnFly.Should().NotBeNull();
        createdOnFly!.StoreId.Should().Be(5);
    }

    [Fact]
    public async Task CreateCustomerAsync_And_UpdateCustomerStoreAsync_ManageCustomerRecord()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var user = new User { UserId = 30, FirstName = "CustomerMember" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var sut = new UserRepository(context);

        // Act 1 - Create
        await sut.CreateCustomerAsync(30, 1);
        var cust = await context.Customers.FirstOrDefaultAsync(c => c.UserId == 30);
        cust.Should().NotBeNull();
        cust!.StoreId.Should().Be(1);

        // Act 2 - Update existing store
        await sut.UpdateCustomerStoreAsync(30, 3);
        var updated = await context.Customers.FirstOrDefaultAsync(c => c.UserId == 30);
        updated!.StoreId.Should().Be(3);
    }

    [Fact]
    public async Task RefreshToken_Lifecycle_Save_Find_Revoke()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var user = new User { UserId = 1, FirstName = "TokenUser", Email = "token@user.com" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var sut = new UserRepository(context);
        const string rawToken = "super-secret-refresh-token-123";

        // 1. Save
        var expiry = DateTime.UtcNow.AddDays(7);
        await sut.SaveRefreshTokenAsync(1, rawToken, expiry);

        // 2. Find by token
        var foundUser = await sut.GetUserByRefreshTokenAsync(rawToken);
        foundUser.Should().NotBeNull();
        foundUser!.UserId.Should().Be(1);
        foundUser.RefreshTokenRevoked.Should().BeFalse();

        // 3. Revoke
        await sut.RevokeRefreshTokenAsync(rawToken);
        var revokedUser = await context.Users.FindAsync(1);
        revokedUser!.RefreshTokenRevoked.Should().BeTrue();
        revokedUser.RefreshTokenRevokedAt.Should().NotBeNull();

        // 4. Revoke by UserId
        await sut.SaveRefreshTokenAsync(1, "token-2", expiry);
        await sut.RevokeRefreshTokenByUserIdAsync(1);
        var revokedAgain = await context.Users.FindAsync(1);
        revokedAgain!.RefreshTokenRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteStaffByUserIdAsync_And_DeleteCustomerByUserIdAsync_SoftDeleteAndUnlink()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Staff.Add(new Staff { StaffId = 1, UserId = 10, StoreId = 1, IsDeleted = false });
        context.Customers.Add(new Customer { CustomerId = 1, UserId = 20, StoreId = 1, IsDeleted = false });
        await context.SaveChangesAsync();

        var sut = new UserRepository(context);

        // Act
        await sut.DeleteStaffByUserIdAsync(10);
        await sut.DeleteCustomerByUserIdAsync(20);

        // Assert
        var staff = await context.Staff.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.StaffId == 1);
        staff!.IsDeleted.Should().BeTrue();
        staff.UserId.Should().BeNull();

        var customer = await context.Customers.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CustomerId == 1);
        customer!.IsDeleted.Should().BeTrue();
        customer.UserId.Should().BeNull();
        customer.Active.Should().Be(0);
    }

    [Fact]
    public async Task CreateAddressAsync_InsertsAddressAndReturnsGeneratedId()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var sut = new UserRepository(context);
        var address = new Address { Street = "Gotham Ave", PostalCode = "10001", Phone = "555-0199", CityId = 1 };

        // Act
        var id = await sut.CreateAddressAsync(address);

        // Assert
        id.Should().BeGreaterThan(0);
        var inDb = await context.Addresses.FindAsync(id);
        inDb.Should().NotBeNull();
        inDb!.Street.Should().Be("Gotham Ave");
    }
}
