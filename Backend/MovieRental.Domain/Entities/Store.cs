using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents a physical DVD rental store location
    [Table("store")]
    public class Store
    {
        [Key]
        [Column("store_id")]
        public int StoreId { get; set; }

        // FK → Staff (the staff member who manages this store)
        [Column("manager_staff_id")]
        [ForeignKey("ManagerStaff")]
        public int ManagerStaffId { get; set; }
        public Staff ManagerStaff { get; set; } = null!;

        // FK → Address
        [Column("address_id")]
        [ForeignKey("Address")]
        public int AddressId { get; set; }
        public Address Address { get; set; } = null!;

        [Column("last_update")]
        public DateTime LastUpdate { get; set; }

        // One Store → Many Customers (registered at this store)
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();

        // One Store → Many Staff (working at this store)
        public ICollection<Staff> Staff { get; set; } = new List<Staff>();

        // One Store → Many Inventory copies
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    }
}
