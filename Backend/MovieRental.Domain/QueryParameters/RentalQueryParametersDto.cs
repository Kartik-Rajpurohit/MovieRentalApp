namespace MovieRental.Domain.QueryParameters;

// Contains options used to filter, sort, and paginate movie rentals returned by the API
public class RentalQueryParametersDto : QueryParametersDto
{
    // Filters rentals made by a specific customer ID
    public int? CustomerId { get; set; }

    // Filters rentals processed by a specific staff member ID
    public int? StaffId { get; set; }

    // Filters rentals for a specific inventory copy ID
    public int? InventoryId { get; set; }

    // Filters rentals by return status (true = returned, false = currently active/not returned)
    public bool? IsReturned { get; set; }

    // Filters rentals by whether payment has been recorded (true = paid, false = unpaid)
    public bool? HasPayment { get; set; }
}
