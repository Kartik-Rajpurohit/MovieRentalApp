using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents a store employee who processes rentals and payments
    // Personal info (name, email, password, address) is stored in the linked User entity
    [Table("staff")]
    public class Staff
    {
        [Key]
        [Column("staff_id")]
        public int StaffId { get; set; }

        // FK → Store (which store this staff member works at)
        [Column("store_id")]
        [ForeignKey("Store")]
        public int StoreId { get; set; }
        public Store Store { get; set; } = null!;

        // Profile photo stored as binary
        [Column("picture")]
        public byte[]? Picture { get; set; }

        // FK → User (staff member's app login account — optional)
        [Column("user_id")]
        [ForeignKey("User")]
        public int? UserId { get; set; }
        public User? User { get; set; }

        // One Staff → Many Rentals (rentals processed by this staff)
        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();

        // One Staff → Many Payments (payments collected by this staff)
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
