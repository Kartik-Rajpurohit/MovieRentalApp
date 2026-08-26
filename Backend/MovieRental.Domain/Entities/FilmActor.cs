using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Junction table resolving many-to-many between Film and Actor
    // Composite PK: (FilmId, ActorId) — configured in AppDbContext
    [Table("film_actor")]
    public class FilmActor
    {
        // FK → Actor (part of composite PK)
        [Column("actor_id")]
        [ForeignKey("Actor")]
        public int ActorId { get; set; }
        public Actor Actor { get; set; } = null!;

        // FK → Film (part of composite PK)
        [Column("film_id")]
        [ForeignKey("Film")]
        public int FilmId { get; set; }
        public Film Film { get; set; } = null!;

        [Column("last_update")]
        public DateTime LastUpdate { get; set; }
    }
}
