namespace MovieRental.Domain.DTOs.Actors;

// Response DTO containing detailed information about an actor, including total films
public class ActorDetailDto
{
    // Unique ID of the actor
    public int ActorId { get; set; }

    // First name of the actor
    public string FirstName { get; set; } = string.Empty;

    // Last name of the actor
    public string LastName { get; set; } = string.Empty;

    // Helper property combining first and last name for display
    public string FullName => $"{FirstName} {LastName}".Trim();

    // Timestamp when the actor was last modified in the database
    public DateTime LastUpdate { get; set; }

    // Total number of films this actor has appeared in
    public int FilmCount { get; set; }
}