using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Customers;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    // Handles business logic for customer profile querying, store assignments, and rental history.
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        // Receives the customer repository needed to query customer data.
        public CustomerService(ICustomerRepository customerRepository) => _customerRepository = customerRepository;

        // Retrieves a paginated and filtered list of customers with search and store filters.
        public async Task<PaginatedResponseDto<CustomerResponseDto>> GetAllCustomersAsync(
            int page, int pageSize, string? search, bool? isActive, int? storeId = null)
        {
            // Get the base query from the repository.
            var query = _customerRepository.GetAllCustomers();

            // Filter by search text matching customer full name, email, or numeric ID.
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                query = query.Where(c =>
                    c.User != null && (
                        (c.User.FirstName + " " + c.User.LastName).ToLower().Contains(lower) ||
                        c.User.Email.ToLower().Contains(lower)
                    ) || c.CustomerId.ToString().Contains(lower)
                );
            }

            // Filter by active account status if specified.
            if (isActive.HasValue)
                query = query.Where(c => c.User != null && c.User.IsActive == isActive.Value);

            // Filter by assigned store location.
            if (storeId.HasValue)
                query = query.Where(c => c.StoreId == storeId.Value);

            var totalRecords = await query.CountAsync();

            // Paginate and project customer entities to response DTOs.
            var data = await query
                .OrderBy(c => c.CustomerId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Data = data
            };
        }

        // Retrieves detailed customer profile by ID including address and location information.
        public async Task<CustomerDetailDto?> GetCustomerByIdAsync(int id)
        {
            var c = await _customerRepository.GetCustomerByIdAsync(id);
            if (c == null) return null;

            // Map database entity and nested user/address navigation properties to DTO.
            return new CustomerDetailDto
            {
                CustomerId = c.CustomerId,
                FullName = c.User != null ? (c.User.FirstName + " " + c.User.LastName).Trim() : "—",
                Email = c.User?.Email,
                IsActive = c.User?.IsActive ?? false,
                StoreId = c.StoreId,
                CreateDate = c.CreateDate,
                Street = c.User?.Address?.Street,
                PostalCode = c.User?.Address?.PostalCode,
                Phone = c.User?.Address?.Phone,
                CityName = c.User?.Address?.City?.Name,
                CountryName = c.User?.Address?.City?.Country?.Name,
            };
        }
    }
}