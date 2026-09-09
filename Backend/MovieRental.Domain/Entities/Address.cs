using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents a physical street address used by users and stores.
    [Table("address")]
    public class Address
    {
        // Primary key uniquely identifying the address.
        [Key]
        [Column("address_id")]
        public int AddressId { get; set; }

        // Primary street address line.
        [Column("address")]
        public string Street { get; set; } = string.Empty;

        // Foreign key linking this address to its city.
        [Column("city_id")]
        [ForeignKey("City")]
        public int CityId { get; set; }

        // Navigation property for the associated city.
        public City City { get; set; } = null!;

        // Postal or ZIP code for postal routing.
        [Column("postal_code")]
        public string? PostalCode { get; set; }

        // Contact phone number associated with this address.
        [Column("phone")]
        public string Phone { get; set; } = string.Empty;

        // Timestamp of when this record was last modified.
        [Column("last_update")]
        public DateTime LastUpdate { get; set; }

        // Collection of users residing at or associated with this address.
        public ICollection<User> Users { get; set; } = new List<User>();

        // Collection of stores located at this address.
        public ICollection<Store> Stores { get; set; } = new List<Store>();
    }
}
