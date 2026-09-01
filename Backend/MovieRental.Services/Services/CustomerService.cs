using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Customers;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        public CustomerService(ICustomerRepository customerRepository) => _customerRepository = customerRepository;

        public async Task<PaginatedResponseDto<CustomerResponseDto>> GetAllCustomersAsync(
            int page, int pageSize, string? search, bool? isActive, int? storeId = null)
        {
            var query = _customerRepository.GetAllCustomers();

            // Filter by search — matches full name, email, or customer ID
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

            // Filter by active status
            if (isActive.HasValue)
                query = query.Where(c => c.User != null && c.User.IsActive == isActive.Value);

            if (storeId.HasValue)
                query = query.Where(c => c.StoreId == storeId.Value);

            var totalRecords = await query.CountAsync();

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

        public async Task<CustomerDetailDto?> GetCustomerByIdAsync(int id)
        {
            var c = await _customerRepository.GetCustomerByIdAsync(id);
            if (c == null) return null;

            // Map raw entity to DTO — service layer responsibility
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