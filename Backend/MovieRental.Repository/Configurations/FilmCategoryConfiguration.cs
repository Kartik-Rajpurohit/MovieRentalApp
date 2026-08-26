using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Configurations
{
    // Configuration for FilmCategory junction table — composite PK and relationships with Film and Category
    public class FilmCategoryConfiguration : IEntityTypeConfiguration<FilmCategory>
    {
        public void Configure(EntityTypeBuilder<FilmCategory> builder)
        {
            // Composite primary key — one film can belong to a category only once
            builder.HasKey(fc => new { fc.FilmId, fc.CategoryId });

            // Many FilmCategories → One Film
            builder.HasOne(fc => fc.Film)
                .WithMany(f => f.FilmCategories)
                .HasForeignKey(fc => fc.FilmId);

            // Many FilmCategories → One Category
            builder.HasOne(fc => fc.Category)
                .WithMany(c => c.FilmCategories)
                .HasForeignKey(fc => fc.CategoryId);
        }
    }
}
