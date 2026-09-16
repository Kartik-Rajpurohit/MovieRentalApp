using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents a movie available in the catalog for rental.
    // Maps to the existing PostgreSQL 'film' table in the Sakila database schema.
    [Table("film")]
    public class Movie
    {
        // Primary key uniquely identifying the movie. Maps to existing database column 'film_id'.
        [Key]
        [Column("film_id")]
        public int MovieId { get; set; }

        // Title of the movie.
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        // Brief synopsis or plot summary of the movie.
        [Column("description")]
        public string? Description { get; set; }

        // The four-digit year the movie was released in theaters.
        [Column("release_year")]
        public int? ReleaseYear { get; set; }

        // Foreign key linking to the movie's primary spoken or dubbed language.
        [Column("language_id")]
        [ForeignKey("Language")]
        public int LanguageId { get; set; }

        // Navigation property for the primary language.
        public Language Language { get; set; } = null!;

        // Foreign key linking to the original language (if translated/dubbed).
        [Column("original_language_id")]
        [ForeignKey("OriginalLanguage")]
        public int? OriginalLanguageId { get; set; }

        // Navigation property for the original language.
        public Language? OriginalLanguage { get; set; }

        // Standard allowed rental duration in days before late return fees apply.
        [Column("rental_duration")]
        public short RentalDuration { get; set; } = 3;

        // Base rental charge for the standard rental duration.
        [Column("rental_rate")]
        public decimal RentalRate { get; set; } = 4.99m;

        // Total running time of the movie in minutes.
        [Column("length")]
        public short? Length { get; set; }

        // Fee charged to the customer if the disc is lost or destroyed.
        [Column("replacement_cost")]
        public decimal ReplacementCost { get; set; } = 19.99m;

        // Official age suitability rating (e.g. G, PG, PG-13, R, NC-17).
        [Column("rating")]
        public string? Rating { get; set; }

        // Timestamp of when this record was last modified.
        [Column("last_update")]
        public DateTime LastUpdate { get; set; }

        // Array of bonus features included on the physical media (e.g. Trailers, Commentaries).
        [Column("special_features")]
        public string[]? SpecialFeatures { get; set; }

        // Navigation property linking the movie to its cast via the film_actor junction table.
        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();

        // Navigation property linking the movie to its genres via the film_category junction table.
        public ICollection<MovieCategory> MovieCategories { get; set; } = new List<MovieCategory>();

        // Physical inventory copies of this movie stocked across all stores.
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    }
}
