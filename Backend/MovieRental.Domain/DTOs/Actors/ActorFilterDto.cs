namespace MovieRental.Domain.DTOs.Actors
{
    // Module-specific filter parameters for actor listings
    public class ActorFilterDto
    {
        // Filters by specific actor ID
        public int? ActorId { get; set; }
    }
}
