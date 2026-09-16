using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents an actor who can appear in multiple movies.
    [Table("actor")]
    public class Actor
    {
        // Primary key uniquely identifying the actor.
        [Key]
        [Column("actor_id")]
        public int ActorId { get; set; }

        // Actor's first name.
        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        // Actor's last or family name.
        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;

        // Timestamp of when this record was last modified in the database.
        [Column("last_update")]
        public DateTime LastUpdate { get; set; }

        // Soft delete flag indicating whether this actor is active or deleted.
        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation property linking the actor to movies via the film_actor junction table.
        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}
