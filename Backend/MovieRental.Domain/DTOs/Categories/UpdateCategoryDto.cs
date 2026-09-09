namespace MovieRental.Domain.DTOs.Categories;

// Request DTO containing data sent by the client to update an existing category
public class UpdateCategoryDto
{
    // Unique ID of the category to update
    public int CategoryId { get; set; }

    // Updated name of the category
    public string Name { get; set; } = string.Empty;
}