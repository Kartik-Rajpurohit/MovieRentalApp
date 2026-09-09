namespace MovieRental.Domain.QueryParameters;

// Contains options used to filter, sort, and paginate languages returned by the API
public class LanguageQueryParametersDto : QueryParametersDto
{
    // Filters languages by language name (e.g. "English", "Italian")
    public string? Name { get; set; }
}
