namespace MovieRental.Domain.DTOs.Actors;

public class UpdateActorDto
{
    public int ActorId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}