namespace MovieRental.Domain.DTOs.Dashboard;

// Response DTO containing film details and total rental count for top rented rankings
public class TopFilmDto
{
    // Unique ID of the film
    public int FilmId { get; set; }

    // Film title
    public string Title { get; set; } = string.Empty;

    // Number of times this film has been rented
    public int RentalCount { get; set; }
}