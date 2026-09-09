namespace MovieRental.Domain.QueryParameters;

// Contains options used to filter, sort, and paginate application users returned by the API
public class UserQueryParametersDto : QueryParametersDto
{
    // Filters users assigned to a specific role ID (e.g. 1=Admin, 2=Staff, 3=Customer)
    public int? RoleId { get; set; }

    // Filters users by first or last name
    public string? Name { get; set; }

    // Filters users by email address
    public string? Email { get; set; }

    // Filters users by active status (true = active, false = deactivated)
    public bool? IsActive { get; set; }
}