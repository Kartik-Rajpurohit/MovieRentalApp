namespace MovieRental.Domain.DTOs.Dashboard;

// Response DTO containing movie details and total rental count for top rented rankings
public class TopMovieDto
{
    // Unique ID of the movie
    public int MovieId { get; set; }

    // Movie title
    public string Title { get; set; } = string.Empty;

    // Number of times this movie has been rented
    public int RentalCount { get; set; }
}
