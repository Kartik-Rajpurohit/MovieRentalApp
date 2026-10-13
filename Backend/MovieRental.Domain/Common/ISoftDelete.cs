namespace MovieRental.Domain.Common;

// Marks an entity as supporting soft-delete filtering via the IsDeleted flag.
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}
