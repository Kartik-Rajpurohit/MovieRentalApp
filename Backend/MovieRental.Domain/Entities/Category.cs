using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents a film genre/category e.g. Action, Comedy, Drama
    [Table("category")]
    public class Category
    {
        [Key]
        [Column("category_id")]
        public int CategoryId { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("last_update")]
        public DateTime LastUpdate { get; set; }

        // One Category → Many FilmCategories (many-to-many with Film via FilmCategory)
        public ICollection<FilmCategory> FilmCategories { get; set; } = new List<FilmCategory>();
    }
}
