using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Customers;

namespace MovieRental.Services.Interfaces
{
    // Defines business operations for customer querying and rental history inspection.
    public interface ICustomerService
    {
        // Retrieves a paginated and filtered list of customers.
        Task<PaginatedResponseDto<CustomerResponseDto>> GetAllCustomersAsync(int page, int pageSize, string? search, bool? isActive, int? storeId = null);

        // Retrieves detailed customer information by ID including rental and payment history.
        Task<CustomerDetailDto?> GetCustomerByIdAsync(int id);
    }
}