using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Junction table resolving the many-to-many relationship between Film and Category.
    // Has a composite primary key consisting of (FilmId, CategoryId).
    [Table("film_category")]
    public class FilmCategory
    {
        // Foreign key linking to the film.
        [Column("film_id")]
        [ForeignKey("Film")]
        public int FilmId { get; set; }

        // Navigation property for the film.
        public Film Film { get; set; } = null!;

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
