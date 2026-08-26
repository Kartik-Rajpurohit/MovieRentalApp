namespace MovieRental.Domain.DTOs.Actors;

public class ActorResponseDto
{
    public int ActorId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public DateTime LastUpdate { get; set; }
    public int FilmCount { get; set; }
}