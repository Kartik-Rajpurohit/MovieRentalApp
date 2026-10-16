using Microsoft.EntityFrameworkCore;
using MovieRental.Repository.Data;

namespace MovieRental.Tests.Unit.Repositories;

public static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new AppDbContext(options);
    }
}
