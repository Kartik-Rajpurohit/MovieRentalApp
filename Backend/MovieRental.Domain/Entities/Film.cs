using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents a film available for rental in the store
    [Table("film")]
    public class Film
    {
        [Key]
        [Column("film_id")]
        public int FilmId { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("release_year")]
        public int? ReleaseYear { get; set; }

        // FK → Language (primary/dubbed language — required)
        [Column("language_id")]
        [ForeignKey("Language")]
        public int LanguageId { get; set; }
        public Language Language { get; set; } = null!;

        // FK → Language (original language — optional)
        [Column("original_language_id")]
        [ForeignKey("OriginalLanguage")]
        public int? OriginalLanguageId { get; set; }
        public Language? OriginalLanguage { get; set; }

        // Number of days the film can be rented
        [Column("rental_duration")]
        public short RentalDuration { get; set; } = 3;

        // Cost per rental period
        [Column("rental_rate")]
        public decimal RentalRate { get; set; } = 4.99m;

        // Film length in minutes
        [Column("length")]
        public short? Length { get; set; }

        // Cost to replace the DVD if lost or damaged
        [Column("replacement_cost")]
        public decimal ReplacementCost { get; set; } = 19.99m;

        // MPAA rating: G, PG, PG-13, R, NC-17
        [Column("rating")]
        public string? Rating { get; set; }

        [Column("last_update")]
        public DateTime LastUpdate { get; set; }

        // PostgreSQL text[] array of special features e.g. Trailers, Commentaries
        [Column("special_features")]
        public string[]? SpecialFeatures { get; set; }

        // One Film → Many FilmActors (many-to-many with Actor via FilmActor)
        public ICollection<FilmActor> FilmActors { get; set; } = new List<FilmActor>();

        // One Film → Many FilmCategories (many-to-many with Category via FilmCategory)
        public ICollection<FilmCategory> FilmCategories { get; set; } = new List<FilmCategory>();

        // One Film → Many Inventory records (multiple physical copies)
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    }
}
