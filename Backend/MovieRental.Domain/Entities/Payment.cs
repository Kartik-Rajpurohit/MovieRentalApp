using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents a monetary payment transaction made by a customer for a rental.
    [Table("payment")]
    public class Payment
    {
        // Primary key uniquely identifying the payment transaction.
        [Key]
        [Column("payment_id")]
        public int PaymentId { get; set; }

        // Foreign key linking to the customer who paid.
        [Column("customer_id")]
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        // Navigation property for the paying customer.
        public Customer Customer { get; set; } = null!;

        // Foreign key linking to the staff member who processed the transaction.
        [Column("staff_id")]
        [ForeignKey("Staff")]
        public int StaffId { get; set; }

        // Navigation property for the staff member who collected the payment.
        public Staff Staff { get; set; } = null!;

        // Foreign key linking to the rental transaction this payment settles.
        [Column("rental_id")]
        [ForeignKey("Rental")]
        public int RentalId { get; set; }

        // Navigation property for the associated rental transaction.
        public Rental Rental { get; set; } = null!;

        // Monetary amount paid in dollars and cents.
        [Column("amount")]
        public decimal Amount { get; set; }

        // Timestamp of when the payment was successfully processed.
        [Column("payment_date")]
        public DateTime PaymentDate { get; set; }
    }
}
