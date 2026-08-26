using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents a customer who rents films from the store
    // Personal info (name, email, address) is stored in the linked User entity
    [Table("customer")]
    public class Customer
    {
        [Key]
        [Column("customer_id")]
        public int CustomerId { get; set; }

        // FK → Store (customer's registered store)
        [Column("store_id")]
        [ForeignKey("Store")]
        public int StoreId { get; set; }
        public Store Store { get; set; } = null!;

        [Column("create_date")]
        public DateOnly CreateDate { get; set; }

        // Integer active flag (legacy Sakila column)
        [Column("active")]
        public int? Active { get; set; }

        // FK → User (customer's app login account — optional)
        [Column("user_id")]
        [ForeignKey("User")]
        public int? UserId { get; set; }
        public User? User { get; set; }

        // One Customer → Many Rentals
        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();

        // One Customer → Many Payments
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
