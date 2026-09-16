using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Configurations
{
    // Configuration for MovieActor junction table — composite PK and relationships with Movie and Actor. Maps to 'film_actor'.
    public class MovieActorConfiguration : IEntityTypeConfiguration<MovieActor>
    {
        public void Configure(EntityTypeBuilder<MovieActor> builder)
        {
            builder.ToTable("film_actor");

            // Composite primary key — one actor can appear in a movie only once
            builder.HasKey(ma => new { ma.MovieId, ma.ActorId });

            // Many MovieActors → One Movie
            builder.HasOne(ma => ma.Movie)
                .WithMany(m => m.MovieActors)
                .HasForeignKey(ma => ma.MovieId);

            // Many MovieActors → One Actor
            builder.HasOne(ma => ma.Actor)
                .WithMany(a => a.MovieActors)
                .HasForeignKey(ma => ma.ActorId);
        }
    }
}
