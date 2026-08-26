using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Configurations
{
    // Configuration for FilmActor junction table — composite PK and relationships with Film and Actor
    public class FilmActorConfiguration : IEntityTypeConfiguration<FilmActor>
    {
        public void Configure(EntityTypeBuilder<FilmActor> builder)
        {
            // Composite primary key — one actor can appear in a film only once
            builder.HasKey(fa => new { fa.FilmId, fa.ActorId });

            // Many FilmActors → One Film
            builder.HasOne(fa => fa.Film)
                .WithMany(f => f.FilmActors)
                .HasForeignKey(fa => fa.FilmId);

            // Many FilmActors → One Actor
            builder.HasOne(fa => fa.Actor)
                .WithMany(a => a.FilmActors)
                .HasForeignKey(fa => fa.ActorId);
        }
    }
}
