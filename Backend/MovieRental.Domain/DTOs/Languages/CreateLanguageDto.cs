using System.ComponentModel.DataAnnotations;

namespace MovieRental.Domain.DTOs.Languages;

// Request DTO sent by the client to add a new movie language
public class CreateLanguageDto
{
    // Name of the language (e.g. "English", "Spanish")
    [Required]
    [StringLength(20)]
    public string Name { get; set; } = string.Empty;
}

