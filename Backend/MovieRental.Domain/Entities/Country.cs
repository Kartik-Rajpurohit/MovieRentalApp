using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents a country — top level of location hierarchy (Country → City → Address)
    [Table("country")]
    public class Country
    {
        [Key]
        [Column("country_id")]
        public int CountryId { get; set; }

        // Column name in DB is "country" not "name"
        [Column("country")]
        public string Name { get; set; } = string.Empty;

        [Column("last_update")]
        public DateTime LastUpdate { get; set; }

        // One Country → Many Cities
        public ICollection<City> Cities { get; set; } = new List<City>();
    }
}
