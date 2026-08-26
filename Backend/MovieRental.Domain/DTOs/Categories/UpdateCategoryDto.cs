namespace MovieRental.Domain.DTOs.Categories;

public class UpdateCategoryDto
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
}