using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Customers;

namespace MovieRental.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<PaginatedResponseDto<CustomerResponseDto>> GetAllCustomersAsync(int page, int pageSize, string? search, bool? isActive);
        Task<CustomerDetailDto?> GetCustomerByIdAsync(int id);
    }
}