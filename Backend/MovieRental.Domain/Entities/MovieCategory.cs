using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Junction table resolving the many-to-many relationship between Movie and Category.
    // Has a composite primary key consisting of (MovieId, CategoryId).
    // Maps to existing PostgreSQL 'film_category' table.
    [Table("film_category")]
    public class MovieCategory
    {
        // Foreign key linking to the movie. Maps to column 'film_id'.
        [Column("film_id")]
        [ForeignKey("Movie")]
        public int MovieId { get; set; }

        // Navigation property for the movie.
        public Movie Movie { get; set; } = null!;

        // Foreign key linking to the category or genre.
        [Column("category_id")]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        // Navigation property for the assigned category.
        public Category Category { get; set; } = null!;

        // Timestamp of when this categorization was last updated.
        [Column("last_update")]
        public DateTime LastUpdate { get; set; }
    }
}
