namespace MovieRental.Domain.DTOs.Rentals;

public class RentalResponseDto
{
    public int RentalId { get; set; }
    public DateTime RentalDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public bool IsReturned => ReturnDate.HasValue;

    public int InventoryId { get; set; }
    public int FilmId { get; set; }
    public string FilmTitle { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;

    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;

    public DateTime LastUpdate { get; set; }
}
