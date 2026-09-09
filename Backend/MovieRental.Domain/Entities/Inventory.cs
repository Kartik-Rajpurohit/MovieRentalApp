using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents a physical copy of a DVD or movie stocked at a specific store.
    [Table("inventory")]
    public class Inventory
    {
        // Primary key uniquely identifying this specific physical movie copy.
        [Key]
        [Column("inventory_id")]
        public int InventoryId { get; set; }

        // Foreign key linking to the film this copy is an instance of.
        [Column("film_id")]
        [ForeignKey("Film")]
        public int FilmId { get; set; }

        // Navigation property for the movie film.
        public Film Film { get; set; } = null!;

        // Foreign key linking to the physical store where this copy is held.
        [Column("store_id")]
        [ForeignKey("Store")]
        public int StoreId { get; set; }

        // Navigation property for the stocking store.
        public Store Store { get; set; } = null!;

        // Timestamp of when this inventory record was last modified.
        [Column("last_update")]
        public DateTime LastUpdate { get; set; }

        // Collection of all rental transactions involving this specific copy over time.
        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    }
}
