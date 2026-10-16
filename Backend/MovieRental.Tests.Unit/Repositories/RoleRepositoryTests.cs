using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Repositories;
using Xunit;

namespace MovieRental.Tests.Unit.Repositories;

public class RoleRepositoryTests
{
    [Fact]
    public async Task GetAllRoles_ReturnsAllRolesWithoutTracking()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Roles.AddRange(
            new Role { RoleId = 1, RoleName = "Admin" },
            new Role { RoleId = 2, RoleName = "Staff" }
        );
        await context.SaveChangesAsync();

        var sut = new RoleRepository(context);

        // Act
        var roles = await sut.GetAllRoles().ToListAsync();

        // Assert
        roles.Should().HaveCount(2);
        roles.Select(r => r.RoleName).Should().Contain(new[] { "Admin", "Staff" });
    }

    [Theory]
    [InlineData("Admin", true)]
    [InlineData("admin", true)]
    [InlineData("ADMIN", true)]
    [InlineData("NonExistent", false)]
    public async Task RoleExistsAsync_MatchesCaseInsensitively(string roleName, bool expectedResult)
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Roles.Add(new Role { RoleId = 1, RoleName = "Admin" });
        await context.SaveChangesAsync();

        var sut = new RoleRepository(context);

        // Act
        var exists = await sut.RoleExistsAsync(roleName);

        // Assert
        exists.Should().Be(expectedResult);
    }

    [Fact]
    public async Task CreateRoleAsync_AddsAndPersistsRole()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var sut = new RoleRepository(context);
        var newRole = new Role { RoleName = "Moderator" };

        // Act
        var created = await sut.CreateRoleAsync(newRole);

        // Assert
        created.RoleId.Should().BeGreaterThan(0);
        var saved = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Moderator");
        saved.Should().NotBeNull();
    }
}
