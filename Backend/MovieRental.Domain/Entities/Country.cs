using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents a nation or country at the top level of the location hierarchy.
    [Table("country")]
    public class Country
    {
        // Primary key uniquely identifying the country.
        [Key]
        [Column("country_id")]
        public int CountryId { get; set; }

        // Name of the country (mapped to column "country" in the database).
        [Column("country")]
        public string Name { get; set; } = string.Empty;

        // Timestamp of when this record was last modified.
        [Column("last_update")]
        public DateTime LastUpdate { get; set; }

        // Collection of cities located within this country.
        public ICollection<City> Cities { get; set; } = new List<City>();
    }
}
