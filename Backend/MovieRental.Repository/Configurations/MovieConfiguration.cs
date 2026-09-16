using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Configurations
{
    // Configuration for Movie entity — maps to 'film' database table and defines relationships with Language
    public class MovieConfiguration : IEntityTypeConfiguration<Movie>
    {
        public void Configure(EntityTypeBuilder<Movie> builder)
        {
            builder.ToTable("film");

            // Many Movies → One primary Language (required)
            builder.HasOne(m => m.Language)
                .WithMany(l => l.Movies)
                .HasForeignKey(m => m.LanguageId);

            // Many Movies → One original Language (optional)
            builder.HasOne(m => m.OriginalLanguage)
                .WithMany(l => l.OriginalLanguageMovies)
                .HasForeignKey(m => m.OriginalLanguageId);

            // Indexes
            builder.HasIndex(m => m.Title);
        }
    }
}
