using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents a physical address used by Users and Stores
    [Table("address")]
    public class Address
    {
        [Key]
        [Column("address_id")]
        public int AddressId { get; set; }

        // Primary street line
        [Column("address")]
        public string Street { get; set; } = string.Empty;

        // FK → City
        [Column("city_id")]
        [ForeignKey("City")]
        public int CityId { get; set; }
        public City City { get; set; } = null!;

        [Column("postal_code")]
        public string? PostalCode { get; set; }

        [Column("phone")]
        public string Phone { get; set; } = string.Empty;

        [Column("last_update")]
        public DateTime LastUpdate { get; set; }

        // One Address → Many Users
        public ICollection<User> Users { get; set; } = new List<User>();

        // One Address → Many Stores
        public ICollection<Store> Stores { get; set; } = new List<Store>();
    }
}
