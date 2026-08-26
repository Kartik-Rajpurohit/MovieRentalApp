using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents a film rental transaction — customer borrowing a specific inventory copy
    [Table("rental")]
    public class Rental
    {
        [Key]
        [Column("rental_id")]
        public int RentalId { get; set; }

        // Date and time when the film was rented out
        [Column("rental_date")]
        public DateTime RentalDate { get; set; }

        // FK → Inventory (specific physical copy that was rented)
        [Column("inventory_id")]
        [ForeignKey("Inventory")]
        public int InventoryId { get; set; }
        public Inventory Inventory { get; set; } = null!;

        // FK → Customer (who rented the film)
        [Column("customer_id")]
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        // Date and time when the film was returned — null means still rented out
        [Column("return_date")]
        public DateTime? ReturnDate { get; set; }

        // FK → Staff (who processed the rental)
        [Column("staff_id")]
        [ForeignKey("Staff")]
        public int StaffId { get; set; }
        public Staff Staff { get; set; } = null!;

        [Column("last_update")]
        public DateTime LastUpdate { get; set; }

        // One Rental → Many Payments (e.g. advance + late fee)
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
