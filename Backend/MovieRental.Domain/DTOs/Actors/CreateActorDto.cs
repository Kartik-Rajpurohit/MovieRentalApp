namespace MovieRental.Domain.DTOs.Actors;

// Request DTO containing data sent by the client to create a new actor
public class CreateActorDto
{
    // First name of the new actor
    public string FirstName { get; set; } = string.Empty;

    // Last name of the new actor
    public string LastName { get; set; } = string.Empty;
}