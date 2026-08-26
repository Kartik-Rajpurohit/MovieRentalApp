using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents a physical copy of a Film available at a specific Store
    [Table("inventory")]
    public class Inventory
    {
        [Key]
        [Column("inventory_id")]
        public int InventoryId { get; set; }

        // FK → Film (which film this copy belongs to)
        [Column("film_id")]
        [ForeignKey("Film")]
        public int FilmId { get; set; }
        public Film Film { get; set; } = null!;

        // FK → Store (which store holds this copy)
        [Column("store_id")]
        [ForeignKey("Store")]
        public int StoreId { get; set; }
        public Store Store { get; set; } = null!;

        [Column("last_update")]
        public DateTime LastUpdate { get; set; }

        // One Inventory copy → Many Rentals over time
        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    }
}
