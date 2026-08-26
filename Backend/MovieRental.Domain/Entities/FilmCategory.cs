using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Junction table resolving many-to-many between Film and Category
    // Composite PK: (FilmId, CategoryId) — configured in AppDbContext
    [Table("film_category")]
    public class FilmCategory
    {
        // FK → Film (part of composite PK)
        [Column("film_id")]
        [ForeignKey("Film")]
        public int FilmId { get; set; }
        public Film Film { get; set; } = null!;

        // FK → Category (part of composite PK)
        [Column("category_id")]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        [Column("last_update")]
        public DateTime LastUpdate { get; set; }
    }
}
