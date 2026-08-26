using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Configurations
{
    // Configuration for Film entity — defines relationships with Language (primary and original)
    public class FilmConfiguration : IEntityTypeConfiguration<Film>
    {
        public void Configure(EntityTypeBuilder<Film> builder)
        {
            // Many Films → One primary Language (required)
            builder.HasOne(f => f.Language)
                .WithMany(l => l.Films)
                .HasForeignKey(f => f.LanguageId);

            // Many Films → One original Language (optional)
            builder.HasOne(f => f.OriginalLanguage)
                .WithMany(l => l.OriginalLanguageFilms)
                .HasForeignKey(f => f.OriginalLanguageId);
        }
    }
}
