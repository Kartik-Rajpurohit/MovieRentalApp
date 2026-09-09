using System.ComponentModel.DataAnnotations;

namespace MovieRental.Domain.DTOs.Languages;

// Request DTO sent by the client to update an existing language
public class UpdateLanguageDto
{
    // Unique ID of the language to update
    [Required]
    public int LanguageId { get; set; }

    // Updated name of the language
    [Required]
    [StringLength(20)]
    public string Name { get; set; } = string.Empty;
}

