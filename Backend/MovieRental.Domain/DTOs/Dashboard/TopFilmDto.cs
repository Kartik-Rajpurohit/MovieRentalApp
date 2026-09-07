namespace MovieRental.Domain.DTOs.Dashboard;

public class TopFilmDto
{
    public int FilmId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int RentalCount { get; set; }
}