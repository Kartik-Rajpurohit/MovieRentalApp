using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Customers;

namespace MovieRental.Services.Interfaces
{
    // Defines business operations for customer querying and rental history inspection.
    public interface ICustomerService
    {
        // Retrieves a paginated, filtered, and sorted list of customers.
        Task<PaginatedResponseDto<CustomerResponseDto>> GetAllCustomersAsync(PaginationInputDto pagination, CustomerFilterDto filter);

        // Retrieves detailed customer information by ID including rental and payment history.
        Task<CustomerDetailDto?> GetCustomerByIdAsync(int id);
    }
}