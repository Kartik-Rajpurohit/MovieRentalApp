namespace MovieRental.Domain.QueryParameters;

public class RentalQueryParametersDto : QueryParametersDto
{
    public int? CustomerId { get; set; }
    public int? StaffId { get; set; }
    public int? InventoryId { get; set; }
    public bool? IsReturned { get; set; }
}
