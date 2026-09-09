using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents a city within a country, used for address classification.
    [Table("city")]
    public class City
    {
        // Primary key uniquely identifying the city.
        [Key]
        [Column("city_id")]
        public int CityId { get; set; }

        // Name of the city (mapped to column "city" in the database).
        [Column("city")]
        public string Name { get; set; } = string.Empty;

        // Foreign key linking this city to its parent country.
        [Column("country_id")]
        [ForeignKey("Country")]
        public int CountryId { get; set; }

        // Navigation property for the parent country.
        public Country Country { get; set; } = null!;

        // Timestamp of when this record was last modified.
        [Column("last_update")]
        public DateTime LastUpdate { get; set; }

        // Collection of physical addresses located in this city.
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}
