using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Configurations
{
    // Configuration for MovieCategory junction table — composite PK and relationships with Movie and Category. Maps to 'film_category'.
    public class MovieCategoryConfiguration : IEntityTypeConfiguration<MovieCategory>
    {
        public void Configure(EntityTypeBuilder<MovieCategory> builder)
        {
            builder.ToTable("film_category");

            // Composite primary key — one movie can belong to a category only once
            builder.HasKey(mc => new { mc.MovieId, mc.CategoryId });

            // Many MovieCategories → One Movie
            builder.HasOne(mc => mc.Movie)
                .WithMany(m => m.MovieCategories)
                .HasForeignKey(mc => mc.MovieId);

            // Many MovieCategories → One Category
            builder.HasOne(mc => mc.Category)
                .WithMany(c => c.MovieCategories)
                .HasForeignKey(mc => mc.CategoryId);
        }
    }
}
