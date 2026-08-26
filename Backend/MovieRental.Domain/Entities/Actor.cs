using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents an actor who can appear in multiple films
    [Table("actor")]
    public class Actor
    {
        [Key]
        [Column("actor_id")]
        public int ActorId { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;

        [Column("last_update")]
        public DateTime LastUpdate { get; set; }

        // One Actor → Many FilmActors (many-to-many with Film via FilmActor)
        public ICollection<FilmActor> FilmActors { get; set; } = new List<FilmActor>();
    }
}
