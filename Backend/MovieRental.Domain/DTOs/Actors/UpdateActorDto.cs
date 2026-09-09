namespace MovieRental.Domain.DTOs.Actors;

// Request DTO containing data sent by the client to update an existing actor
public class UpdateActorDto
{
    // Unique ID of the actor to update
    public int ActorId { get; set; }

    // Updated first name
    public string FirstName { get; set; } = string.Empty;

    // Updated last name
    public string LastName { get; set; } = string.Empty;
}