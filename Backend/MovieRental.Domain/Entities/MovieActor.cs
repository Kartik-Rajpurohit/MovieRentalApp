using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Junction table resolving the many-to-many relationship between Movie and Actor.
    // Has a composite primary key consisting of (MovieId, ActorId).
    // Maps to existing PostgreSQL 'film_actor' table.
    [Table("film_actor")]
    public class MovieActor
    {
        // Foreign key linking to the participating actor.
        [Column("actor_id")]
        [ForeignKey("Actor")]
        public int ActorId { get; set; }

        // Navigation property for the actor.
        public Actor Actor { get; set; } = null!;

        // Foreign key linking to the movie the actor appeared in. Maps to column 'film_id'.
        [Column("film_id")]
        [ForeignKey("Movie")]
        public int MovieId { get; set; }

        // Navigation property for the movie.
        public Movie Movie { get; set; } = null!;

        // Timestamp of when this casting association was last updated.
        [Column("last_update")]
        public DateTime LastUpdate { get; set; }
    }
}
