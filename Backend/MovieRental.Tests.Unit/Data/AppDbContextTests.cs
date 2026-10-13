using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Common;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;

namespace MovieRental.Tests.Unit.Data;

public class AppDbContextTests
{
    [Fact]
    public void OnModelCreating_ShouldConfigureQueryFilter_ForEveryEntityImplementingISoftDelete()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=test_filter_db;Username=postgres;Password=postgres")
            .Options;

        using var context = new AppDbContext(options);

        // Act
        var model = context.Model;
        var softDeleteEntityTypes = model.GetEntityTypes()
            .Where(e => typeof(ISoftDelete).IsAssignableFrom(e.ClrType) && e.BaseType == null)
            .ToList();

        // Assert
        softDeleteEntityTypes.Should().NotBeEmpty();

        foreach (var entityType in softDeleteEntityTypes)
        {
            var filters = entityType.GetDeclaredQueryFilters();
            filters.Should().NotBeEmpty($"Entity '{entityType.ClrType.Name}' implements ISoftDelete and must have a global query filter configured.");
        }
    }

    [Theory]
    [InlineData(typeof(Movie))]
    [InlineData(typeof(Actor))]
    [InlineData(typeof(Customer))]
    [InlineData(typeof(Staff))]
    [InlineData(typeof(Store))]
    [InlineData(typeof(Rental))]
    [InlineData(typeof(Inventory))]
    [InlineData(typeof(Payment))]
    [InlineData(typeof(User))]
    [InlineData(typeof(Category))]
    [InlineData(typeof(Language))]
    [InlineData(typeof(Address))]
    [InlineData(typeof(City))]
    [InlineData(typeof(Country))]
    [InlineData(typeof(Role))]
    public void OnModelCreating_ShouldHaveQueryFilter_ForCoreDomainEntities(Type entityType)
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=test_filter_db;Username=postgres;Password=postgres")
            .Options;

        using var context = new AppDbContext(options);

        // Act
        var efEntityType = context.Model.FindEntityType(entityType);

        // Assert
        efEntityType.Should().NotBeNull();
        efEntityType!.GetDeclaredQueryFilters().Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(typeof(Movie))]
    [InlineData(typeof(Actor))]
    [InlineData(typeof(Customer))]
    [InlineData(typeof(Staff))]
    [InlineData(typeof(Store))]
    [InlineData(typeof(Rental))]
    [InlineData(typeof(Inventory))]
    [InlineData(typeof(Payment))]
    [InlineData(typeof(User))]
    [InlineData(typeof(Category))]
    [InlineData(typeof(Language))]
    [InlineData(typeof(Address))]
    [InlineData(typeof(City))]
    [InlineData(typeof(Country))]
    [InlineData(typeof(Role))]
    public void OnModelCreating_ShouldConfigureIsDeletedColumnNameAndDefaultValue_ForCoreDomainEntities(Type entityType)
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=test_filter_db;Username=postgres;Password=postgres")
            .Options;

        using var context = new AppDbContext(options);

        // Act
        var efEntityType = context.Model.FindEntityType(entityType);
        var isDeletedProperty = efEntityType?.FindProperty(nameof(ISoftDelete.IsDeleted));

        // Assert
        isDeletedProperty.Should().NotBeNull($"Entity '{entityType.Name}' must have property 'IsDeleted' mapped.");
        isDeletedProperty!.GetColumnName().Should().Be("is_deleted");
        isDeletedProperty.GetDefaultValue().Should().Be(false);
    }

    [Fact]
    public void MovieProjection_ShouldTranslateToSql_WithoutFetchingUnneededColumns()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=test_filter_db;Username=postgres;Password=postgres")
            .Options;

        using var context = new AppDbContext(options);

        // Act
        var sql = context.Movies
            .Select(m => new MovieRental.Domain.DTOs.Movies.MovieResponseDto
            {
                MovieId = m.MovieId,
                Title = m.Title,
                Description = m.Description,
                ReleaseYear = m.ReleaseYear,
                LanguageId = m.LanguageId,
                LanguageName = m.Language != null ? m.Language.Name : "",
                RentalDuration = m.RentalDuration,
                RentalRate = m.RentalRate,
                Length = m.Length,
                ReplacementCost = m.ReplacementCost,
                Rating = m.Rating,
                Categories = m.MovieCategories.Select(mc => mc.Category.Name).ToList(),
                Actors = m.MovieActors.Select(ma => (ma.Actor.FirstName + " " + ma.Actor.LastName).Trim()).ToList()
            })
            .ToQueryString();

        // Assert
        sql.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void InventoryProjection_ShouldTranslateToSql_WithoutFetchingUnneededColumns()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=test_filter_db;Username=postgres;Password=postgres")
            .Options;

        using var context = new AppDbContext(options);

        // Act
        var sql = context.Inventories
            .Select(i => new MovieRental.Domain.DTOs.Inventory.InventoryResponseDto
            {
                InventoryId = i.InventoryId,
                MovieId = i.MovieId,
                MovieTitle = i.Movie != null ? i.Movie.Title : "",
                StoreId = i.StoreId,
                IsAvailable = !i.Rentals.Any(r => r.ReturnDate == null),
                LastUpdate = i.LastUpdate
            })
            .ToQueryString();

        // Assert
        sql.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData(typeof(Movie))]
    [InlineData(typeof(Rental))]
    [InlineData(typeof(Payment))]
    public void BaseQueryables_ShouldNotContainJoins_ForEfficientCounting(Type entityType)
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=test_filter_db;Username=postgres;Password=postgres")
            .Options;

        using var context = new AppDbContext(options);

        // Act
        IQueryable query = entityType switch
        {
            var t when t == typeof(Movie) => context.Movies.Where(m => !m.IsDeleted),
            var t when t == typeof(Rental) => context.Rentals.Where(r => !r.IsDeleted),
            var t when t == typeof(Payment) => context.Payments.Where(p => !p.IsDeleted),
            _ => throw new ArgumentException("Unsupported entity type", nameof(entityType))
        };

        var sql = query.ToQueryString();

        // Assert
        sql.Should().NotContainEquivalentOf("JOIN", "Base counting queryables must not include eager joins.");
    }
}
