namespace MovieRental.Domain.DTOs.Common;

// Response DTO used for populating dropdown lists (select inputs) with ID-Name pairs
public class DropdownDto
{
    // Identifier of the item (value)
    public int Id { get; set; }

    // Display label of the item (text)
    public string Name { get; set; } = string.Empty;
}
