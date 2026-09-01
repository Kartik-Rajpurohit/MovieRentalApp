using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Customers;

namespace MovieRental.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<PaginatedResponseDto<CustomerResponseDto>> GetAllCustomersAsync(int page, int pageSize, string? search, bool? isActive, int? storeId = null);
        Task<CustomerDetailDto?> GetCustomerByIdAsync(int id);
    }
}