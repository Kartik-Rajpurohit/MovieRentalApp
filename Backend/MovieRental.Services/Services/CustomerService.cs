using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Customers;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Extensions;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    // Handles business logic for customer profile querying, store assignments, and rental history.
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        // Receives the customer repository needed to query customer data.
        public CustomerService(ICustomerRepository customerRepository) => _customerRepository = customerRepository;

        // Retrieves a paginated, filtered, and sorted list of customers.
        public async Task<PaginatedResponseDto<CustomerResponseDto>> GetAllCustomersAsync(
            PaginationInputDto pagination, CustomerFilterDto filter)
        {
            // 1. Get the base query from the repository.
            var query = _customerRepository.GetAllCustomers();

            // 2. Search: matching customer full name, email, or numeric ID.
            if (!string.IsNullOrWhiteSpace(pagination.Search))
            {
                var lower = pagination.Search.ToLower();
                query = query.Where(c =>
                    c.User != null && (
                        (c.User.FirstName + " " + c.User.LastName).ToLower().Contains(lower) ||
                        c.User.Email.ToLower().Contains(lower)
                    ) || c.CustomerId.ToString().Contains(lower)
                );
            }

            // 3. Module Filters: account active status and assigned store.
            if (filter.IsActive.HasValue)
                query = query.Where(c => c.User != null && c.User.IsActive == filter.IsActive.Value);

            if (filter.StoreId.HasValue)
                query = query.Where(c => c.StoreId == filter.StoreId.Value);

            // 4. Sorting: dynamic sorting on allowed fields.
            var isDesc = string.Equals(pagination.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
            query = pagination.SortBy?.ToLower() switch
            {
                "firstname" => isDesc
                    ? query.OrderByDescending(c => c.User != null ? c.User.FirstName : "")
                    : query.OrderBy(c => c.User != null ? c.User.FirstName : ""),
                "lastname" => isDesc
                    ? query.OrderByDescending(c => c.User != null ? c.User.LastName : "")
                    : query.OrderBy(c => c.User != null ? c.User.LastName : ""),
                "email" => isDesc
                    ? query.OrderByDescending(c => c.User != null ? c.User.Email : "")
                    : query.OrderBy(c => c.User != null ? c.User.Email : ""),
                "store" or "storeid" => isDesc
                    ? query.OrderByDescending(c => c.StoreId)
                    : query.OrderBy(c => c.StoreId),
                "active" or "isactive" => isDesc
                    ? query.OrderByDescending(c => c.User != null && c.User.IsActive)
                    : query.OrderBy(c => c.User != null && c.User.IsActive),
                "createdate" => isDesc
                    ? query.OrderByDescending(c => c.CreateDate)
                    : query.OrderBy(c => c.CreateDate),
                _ => isDesc
                    ? query.OrderByDescending(c => c.CustomerId)
                    : query.OrderBy(c => c.CustomerId)
            };

            // 5. Total Count: before pagination.
            var totalRecords = await query.CountAsync();

            // 6. Pagination & Projection: database-side execution.
            var data = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(c => new CustomerResponseDto
                {
                    CustomerId = c.CustomerId,
                    FullName = c.User != null ? (c.User.FirstName + " " + c.User.LastName).Trim() : "—",
                    Email = c.User != null ? c.User.Email : null,
                    StoreId = c.StoreId,
                    IsActive = c.User != null && c.User.IsActive,
                    CreateDate = c.CreateDate,
                })
                .ToListAsync();

            return new PaginatedResponseDto<CustomerResponseDto>
            {
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pagination.PageSize),
                CurrentPage = pagination.Page,
                PageSize = pagination.PageSize,
                Data = data
            };
        }

        // Retrieves detailed customer profile by ID including address and location information.
        public async Task<CustomerDetailDto?> GetCustomerByIdAsync(int id)
        {
            var c = await _customerRepository.GetCustomerByIdAsync(id);
            if (c == null) return null;

            return c.ToDetailDto();
        }
    }
}