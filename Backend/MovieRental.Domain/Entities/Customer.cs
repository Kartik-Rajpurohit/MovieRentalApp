using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents a store customer who can rent films and make payments.
    [Table("customer")]
    public class Customer
    {
        // Primary key uniquely identifying the customer.
        [Key]
        [Column("customer_id")]
        public int CustomerId { get; set; }

        // Foreign key linking customer to their home or registered store.
        [Column("store_id")]
        [ForeignKey("Store")]
        public int StoreId { get; set; }

        // Navigation property for the customer's registered store.
        public Store Store { get; set; } = null!;

        // Date when this customer account was originally registered.
        [Column("create_date")]
        public DateOnly CreateDate { get; set; }

        // Integer active flag from legacy Sakila schema (1 = active, 0 = inactive).
        [Column("active")]
        public int? Active { get; set; }

        // Foreign key linking this customer profile to an application user login account.
        [Column("user_id")]
        [ForeignKey("User")]
        public int? UserId { get; set; }

        // Navigation property for the associated application user account.
        public User? User { get; set; }

        // Collection of all rental transactions made by this customer.
        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();

        // Collection of all payments made by this customer.
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
