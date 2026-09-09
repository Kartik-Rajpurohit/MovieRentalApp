namespace MovieRental.Domain.DTOs.Categories;

// Request DTO containing data sent by the client to create a new category
public class CreateCategoryDto
{
    // Name of the new category/genre
    public string Name { get; set; } = string.Empty;
}
