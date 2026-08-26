using System.ComponentModel.DataAnnotations;

namespace MovieRental.Domain.DTOs.Languages;

public class CreateLanguageDto
{
    [Required]
    [StringLength(20)]
    public string Name { get; set; } = string.Empty;
}

