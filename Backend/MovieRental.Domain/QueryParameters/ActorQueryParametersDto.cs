namespace MovieRental.Domain.QueryParameters;

public class ActorQueryParametersDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? SortField { get; set; }
    public string? SortOrder { get; set; }
    public int? ActorId { get; set; }
}