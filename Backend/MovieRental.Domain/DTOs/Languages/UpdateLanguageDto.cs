using System.ComponentModel.DataAnnotations;

namespace MovieRental.Domain.DTOs.Languages;

public class UpdateLanguageDto
{
    [Required]
    public int LanguageId { get; set; }

    [Required]
    [StringLength(20)]
    public string Name { get; set; } = string.Empty;
}

