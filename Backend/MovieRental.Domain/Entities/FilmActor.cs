using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Junction table resolving the many-to-many relationship between Film and Actor.
    // Has a composite primary key consisting of (FilmId, ActorId).
    [Table("film_actor")]
    public class FilmActor
    {
        // Foreign key linking to the participating actor.
        [Column("actor_id")]
        [ForeignKey("Actor")]
        public int ActorId { get; set; }

        // Navigation property for the actor.
        public Actor Actor { get; set; } = null!;

        // Foreign key linking to the movie the actor appeared in.
        [Column("film_id")]
        [ForeignKey("Film")]
        public int FilmId { get; set; }

        // Navigation property for the film.
        public Film Film { get; set; } = null!;

        // Timestamp of when this casting association was last updated.
        [Column("last_update")]
        public DateTime LastUpdate { get; set; }
    }
}
