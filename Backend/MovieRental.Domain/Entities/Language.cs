using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents a spoken/dubbed language for films
    [Table("language")]
    public class Language
    {
        [Key]
        [Column("language_id")]
        public int LanguageId { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("last_update")]
        public DateTime LastUpdate { get; set; }

        // One Language → Many Films (as primary dubbed language)
        public ICollection<Film> Films { get; set; } = new List<Film>();

        // One Language → Many Films (as original language)
        public ICollection<Film> OriginalLanguageFilms { get; set; } = new List<Film>();
    }
}
