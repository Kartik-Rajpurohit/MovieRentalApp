using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents a movie genre or category (such as Action, Comedy, Drama).
    [Table("category")]
    public class Category
    {
        // Primary key uniquely identifying the category.
        [Key]
        [Column("category_id")]
        public int CategoryId { get; set; }

        // Descriptive name of the genre or category.
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        // Timestamp of when this record was last modified.
        [Column("last_update")]
        public DateTime LastUpdate { get; set; }

        // Navigation property linking the category to films via the film_category junction table.
        public ICollection<FilmCategory> FilmCategories { get; set; } = new List<FilmCategory>();
    }
}
