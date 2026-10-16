using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Repositories;
using Xunit;

namespace MovieRental.Tests.Unit.Repositories;

public class InventoryRepositoryTests
{
    [Fact]
    public async Task GetAllInventory_ReturnsActiveInventory()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Inventories.AddRange(
            new Inventory { InventoryId = 1, MovieId = 1, StoreId = 1, IsDeleted = false },
            new Inventory { InventoryId = 2, MovieId = 1, StoreId = 2, IsDeleted = false }
        );
        await context.SaveChangesAsync();

        var sut = new InventoryRepository(context);

        // Act
        var inventory = await sut.GetAllInventory().ToListAsync();

        // Assert
        inventory.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetInventoryByIdAsync_WhenExists_ReturnsInventoryWithMovieAndRentals()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var movie = new Movie { MovieId = 10, Title = "Interstellar" };
        var inventory = new Inventory
        {
            InventoryId = 5,
            MovieId = 10,
            Movie = movie,
            StoreId = 1,
            Rentals = new List<Rental> { new() { RentalId = 100 } }
        };
        context.Movies.Add(movie);
        context.Inventories.Add(inventory);
        await context.SaveChangesAsync();

        var sut = new InventoryRepository(context);

        // Act
        var result = await sut.GetInventoryByIdAsync(5);

        // Assert
        result.Should().NotBeNull();
        result!.Movie.Title.Should().Be("Interstellar");
        result.Rentals.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateInventoryAsync_AddsAndLoadsRelations()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var movie = new Movie { MovieId = 1, Title = "Avatar" };
        context.Movies.Add(movie);
        await context.SaveChangesAsync();

        var sut = new InventoryRepository(context);
        var inventory = new Inventory { MovieId = 1, StoreId = 1 };

        // Act
        var created = await sut.CreateInventoryAsync(inventory);

        // Assert
        created.InventoryId.Should().BeGreaterThan(0);
        created.Movie.Should().NotBeNull();
        created.Movie.Title.Should().Be("Avatar");
    }

    [Fact]
    public async Task UpdateInventoryAsync_WhenExists_UpdatesStoreAssignment()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var movie = new Movie { MovieId = 1, Title = "Gladiator" };
        var inventory = new Inventory { InventoryId = 1, MovieId = 1, Movie = movie, StoreId = 1 };
        context.Movies.Add(movie);
        context.Inventories.Add(inventory);
        await context.SaveChangesAsync();

        var sut = new InventoryRepository(context);
        var updateModel = new Inventory { InventoryId = 1, StoreId = 3 };

        // Act
        var updated = await sut.UpdateInventoryAsync(updateModel);

        // Assert
        updated.Should().NotBeNull();
        updated!.StoreId.Should().Be(3);
    }

    [Fact]
    public async Task DeleteInventoryAsync_WhenExists_SoftDeletesItem()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Inventories.Add(new Inventory { InventoryId = 1, MovieId = 1, StoreId = 1, IsDeleted = false });
        await context.SaveChangesAsync();

        var sut = new InventoryRepository(context);

        // Act
        var success = await sut.DeleteInventoryAsync(1);

        // Assert
        success.Should().BeTrue();
        var inDb = await context.Inventories.IgnoreQueryFilters().FirstOrDefaultAsync(i => i.InventoryId == 1);
        inDb!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task MovieExistsAsync_ValidatesMoviePresence()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Movies.Add(new Movie { MovieId = 1, Title = "Active", IsDeleted = false });
        context.Movies.Add(new Movie { MovieId = 2, Title = "Deleted", IsDeleted = true });
        await context.SaveChangesAsync();

        var sut = new InventoryRepository(context);

        // Act & Assert
        (await sut.MovieExistsAsync(1)).Should().BeTrue();
        (await sut.MovieExistsAsync(2)).Should().BeFalse();
        (await sut.MovieExistsAsync(999)).Should().BeFalse();
    }

    [Fact]
    public async Task StoreExistsAsync_ValidatesStorePresence()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Stores.Add(new Store { StoreId = 1 });
        await context.SaveChangesAsync();

        var sut = new InventoryRepository(context);

        // Act & Assert
        (await sut.StoreExistsAsync(1)).Should().BeTrue();
        (await sut.StoreExistsAsync(999)).Should().BeFalse();
    }
}
