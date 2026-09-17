using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Staff;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Extensions;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    // Handles business logic for staff lookups and store affiliation queries.
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _staffRepository;

        // Receives the staff repository needed to access staff records.
        public StaffService(IStaffRepository staffRepository) => _staffRepository = staffRepository;

        // Retrieves a paginated, filtered, and sorted list of staff members.
        public async Task<PaginatedResponseDto<StaffResponseDto>> GetAllStaffAsync(
            PaginationInputDto pagination, StaffFilterDto filter)
        {
            // 1. Get the base query from the repository.
            var query = _staffRepository.GetAllStaff();

            // 2. Search: matches staff full name, email, or numeric staff ID.
            if (!string.IsNullOrWhiteSpace(pagination.Search))
            {
                var lower = pagination.Search.ToLower();
                query = query.Where(s =>
                    s.User != null && (
                        (s.User.FirstName + " " + s.User.LastName).ToLower().Contains(lower) ||
                        s.User.Email.ToLower().Contains(lower)
                    ) || s.StaffId.ToString().Contains(lower)
                );
            }

            // 3. Module Filters: account active status and assigned store.
            if (filter.IsActive.HasValue)
                query = query.Where(s => s.User != null && s.User.IsActive == filter.IsActive.Value);

            if (filter.StoreId.HasValue)
                query = query.Where(s => s.StoreId == filter.StoreId.Value);

            // 4. Sorting: dynamic sorting on allowed fields.
            var isDesc = string.Equals(pagination.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
            query = pagination.SortBy?.ToLower() switch
            {
                "firstname" => isDesc
                    ? query.OrderByDescending(s => s.User != null ? s.User.FirstName : "")
                    : query.OrderBy(s => s.User != null ? s.User.FirstName : ""),
                "lastname" => isDesc
                    ? query.OrderByDescending(s => s.User != null ? s.User.LastName : "")
                    : query.OrderBy(s => s.User != null ? s.User.LastName : ""),
                "email" => isDesc
                    ? query.OrderByDescending(s => s.User != null ? s.User.Email : "")
                    : query.OrderBy(s => s.User != null ? s.User.Email : ""),
                "store" or "storeid" => isDesc
                    ? query.OrderByDescending(s => s.StoreId)
                    : query.OrderBy(s => s.StoreId),
                "active" or "isactive" => isDesc
                    ? query.OrderByDescending(s => s.User != null && s.User.IsActive)
                    : query.OrderBy(s => s.User != null && s.User.IsActive),
                _ => isDesc
                    ? query.OrderByDescending(s => s.StaffId)
                    : query.OrderBy(s => s.StaffId)
            };

            // 5. Total Count: before pagination.
            var totalRecords = await query.CountAsync();

            // 6. Pagination & Projection: database-side execution.
            var data = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
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
                TotalPages = (int)Math.Ceiling((double)totalRecords / pagination.PageSize),
                CurrentPage = pagination.Page,
                PageSize = pagination.PageSize,
                Data = data
            };
        }

        // Retrieves detailed staff profile by ID including address and assigned store.
        public async Task<StaffDetailDto?> GetStaffByIdAsync(int id)
        {
            var s = await _staffRepository.GetStaffByIdAsync(id);
            if (s == null) return null;

            return s.ToDetailDto();
        }
    }
}