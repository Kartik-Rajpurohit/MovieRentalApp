using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents a city — middle level of location hierarchy (Country → City → Address)
    [Table("city")]
    public class City
    {
        [Key]
        [Column("city_id")]
        public int CityId { get; set; }

        // Column name in DB is "city" not "name"
        [Column("city")]
        public string Name { get; set; } = string.Empty;

        // FK → Country
        [Column("country_id")]
        [ForeignKey("Country")]
        public int CountryId { get; set; }
        public Country Country { get; set; } = null!;

        [Column("last_update")]
        public DateTime LastUpdate { get; set; }

        // One City → Many Addresses
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}
