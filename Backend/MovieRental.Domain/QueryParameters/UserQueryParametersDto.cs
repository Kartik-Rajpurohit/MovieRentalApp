namespace MovieRental.Domain.QueryParameters;

public class UserQueryParametersDto : QueryParametersDto
{
    public int? RoleId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public bool? IsActive { get; set; }
}