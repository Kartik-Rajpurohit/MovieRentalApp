using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Staff;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    // Handles business logic for staff lookups and store affiliation queries.
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _staffRepository;

        // Receives the staff repository needed to access staff records.
        public StaffService(IStaffRepository staffRepository) => _staffRepository = staffRepository;

        // Retrieves a paginated and filtered list of staff members with search and store filters.
        public async Task<PaginatedResponseDto<StaffResponseDto>> GetAllStaffAsync(
            int page, int pageSize, string? search, bool? isActive, int? storeId = null)
        {
            // Get the base query from the repository.
            var query = _staffRepository.GetAllStaff();

            // Filter by search matching staff full name, email, or numeric staff ID.
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                query = query.Where(s =>
                    s.User != null && (
                        (s.User.FirstName + " " + s.User.LastName).ToLower().Contains(lower) ||
                        s.User.Email.ToLower().Contains(lower)
                    ) || s.StaffId.ToString().Contains(lower)
                );
            }

            // Filter by account active status if specified.
            if (isActive.HasValue)
                query = query.Where(s => s.User != null && s.User.IsActive == isActive.Value);

            // Filter by assigned store ID if specified.
            if (storeId.HasValue)
                query = query.Where(s => s.StoreId == storeId.Value);

            var totalRecords = await query.CountAsync();

            // Paginate and project staff entities to response DTOs.
            var data = await query
                .OrderBy(s => s.StaffId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new StaffResponseDto
                {
                    StaffId = s.StaffId,
                    FullName = s.User != null ? (s.User.FirstName + " " + s.User.LastName).Trim() : "—",
                    Email = s.User != null ? s.User.Email : null,
                    StoreId = s.StoreId,
                    IsActive = s.User != null && s.User.IsActive,
                })
                .ToListAsync();

            return new PaginatedResponseDto<StaffResponseDto>
            {
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Data = data
            };
        }

        // Retrieves detailed staff profile by ID including address and assigned store.
        public async Task<StaffDetailDto?> GetStaffByIdAsync(int id)
        {
            var s = await _staffRepository.GetStaffByIdAsync(id);
            if (s == null) return null;

            // Map database entity and nested user/address navigation properties to DTO.
            return new StaffDetailDto
            {
                StaffId = s.StaffId,
                FullName = s.User != null ? (s.User.FirstName + " " + s.User.LastName).Trim() : "—",
                Email = s.User?.Email,
                IsActive = s.User?.IsActive ?? false,
                StoreId = s.StoreId,
                Street = s.User?.Address?.Street,
                PostalCode = s.User?.Address?.PostalCode,
                Phone = s.User?.Address?.Phone,
                CityName = s.User?.Address?.City?.Name,
                CountryName = s.User?.Address?.City?.Country?.Name,
            };
        }
    }
}