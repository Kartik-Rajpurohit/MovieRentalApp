using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Staff;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _staffRepository;
        public StaffService(IStaffRepository staffRepository) => _staffRepository = staffRepository;

        public async Task<PaginatedResponseDto<StaffResponseDto>> GetAllStaffAsync(
            int page, int pageSize, string? search, bool? isActive, int? storeId = null)
        {
            var query = _staffRepository.GetAllStaff();

            // Filter by search — matches full name, email, or staff ID
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

            // Filter by active status
            if (isActive.HasValue)
                query = query.Where(s => s.User != null && s.User.IsActive == isActive.Value);

            if (storeId.HasValue)
                query = query.Where(s => s.StoreId == storeId.Value);

            var totalRecords = await query.CountAsync();

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

        public async Task<StaffDetailDto?> GetStaffByIdAsync(int id)
        {
            var s = await _staffRepository.GetStaffByIdAsync(id);
            if (s == null) return null;

            // Map raw entity to DTO — service layer responsibility
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