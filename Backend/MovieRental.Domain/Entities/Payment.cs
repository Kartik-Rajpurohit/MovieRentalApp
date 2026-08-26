using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents a payment made by a customer for a rental
    [Table("payment")]
    public class Payment
    {
        [Key]
        [Column("payment_id")]
        public int PaymentId { get; set; }

        // FK → Customer (who made the payment)
        [Column("customer_id")]
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        // FK → Staff (who collected the payment)
        [Column("staff_id")]
        [ForeignKey("Staff")]
        public int StaffId { get; set; }
        public Staff Staff { get; set; } = null!;

        // FK → Rental (which rental this payment is for)
        [Column("rental_id")]
        [ForeignKey("Rental")]
        public int RentalId { get; set; }
        public Rental Rental { get; set; } = null!;

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("payment_date")]
        public DateTime PaymentDate { get; set; }
    }
}
