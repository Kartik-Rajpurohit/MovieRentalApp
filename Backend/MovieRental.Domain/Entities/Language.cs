using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Domain.Entities
{
    // Represents a spoken or dubbed language available for movies.
    [Table("language")]
    public class Language
    {
        // Primary key uniquely identifying the language.
        [Key]
        [Column("language_id")]
        public int LanguageId { get; set; }

        // Name of the language (e.g. English, Italian, Japanese).
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        // Timestamp of when this record was last modified.
        [Column("last_update")]
        public DateTime LastUpdate { get; set; }

        // Films where this language is the primary spoken/dubbed audio.
        public ICollection<Film> Films { get; set; } = new List<Film>();

        // Films where this language is the original production audio.
        public ICollection<Film> OriginalLanguageFilms { get; set; } = new List<Film>();
    }
}
