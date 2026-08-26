using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Rentals;
using MovieRental.Domain.Entities;
using MovieRental.Domain.QueryParameters;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    public class RentalService : IRentalService
    {
        private readonly IRentalRepository _rentalRepository;

        public RentalService(IRentalRepository rentalRepository)
        {
            _rentalRepository = rentalRepository;
        }

        private static RentalResponseDto MapToResponse(Rental r) => new()
        {
            RentalId = r.RentalId,
            RentalDate = r.RentalDate,
            ReturnDate = r.ReturnDate,
            InventoryId = r.InventoryId,
            FilmId = r.Inventory?.FilmId ?? 0,
            FilmTitle = r.Inventory?.Film?.Title ?? "",
            CustomerId = r.CustomerId,
            CustomerName = r.Customer?.User != null
                ? $"{r.Customer.User.FirstName} {r.Customer.User.LastName}".Trim()
                : $"Customer {r.CustomerId}",
            StaffId = r.StaffId,
            StaffName = r.Staff?.User != null
                ? $"{r.Staff.User.FirstName} {r.Staff.User.LastName}".Trim()
                : $"Staff {r.StaffId}",
            LastUpdate = r.LastUpdate,
        };

        private static RentalDetailDto MapToDetail(Rental r) => new()
        {
            RentalId = r.RentalId,
            RentalDate = r.RentalDate,
            ReturnDate = r.ReturnDate,
            InventoryId = r.InventoryId,
            FilmId = r.Inventory?.FilmId ?? 0,
            FilmTitle = r.Inventory?.Film?.Title ?? "",
            CustomerId = r.CustomerId,
            CustomerName = r.Customer?.User != null
                ? $"{r.Customer.User.FirstName} {r.Customer.User.LastName}".Trim()
                : $"Customer {r.CustomerId}",
            StaffId = r.StaffId,
            StaffName = r.Staff?.User != null
                ? $"{r.Staff.User.FirstName} {r.Staff.User.LastName}".Trim()
                : $"Staff {r.StaffId}",
            TotalPaid = r.Payments.Sum(p => p.Amount),
            PaymentCount = r.Payments.Count,
            LastUpdate = r.LastUpdate,
        };

        public async Task<PaginatedResponseDto<RentalResponseDto>> GetAllRentalsAsync(RentalQueryParametersDto queryParams)
        {
            var query = _rentalRepository.GetAllRentals();

            // Filters
            if (queryParams.CustomerId.HasValue)
                query = query.Where(r => r.CustomerId == queryParams.CustomerId.Value);

            if (queryParams.StaffId.HasValue)
                query = query.Where(r => r.StaffId == queryParams.StaffId.Value);

            if (queryParams.InventoryId.HasValue)
                query = query.Where(r => r.InventoryId == queryParams.InventoryId.Value);

            if (queryParams.IsReturned.HasValue)
                query = queryParams.IsReturned.Value
                    ? query.Where(r => r.ReturnDate != null)
                    : query.Where(r => r.ReturnDate == null);

            // Search — by film title or customer name
            if (!string.IsNullOrEmpty(queryParams.Search))
            {
                var s = queryParams.Search.ToLower();
                query = query.Where(r =>
                    r.Inventory.Film.Title.ToLower().Contains(s) ||
                    (r.Customer.User.FirstName + " " + r.Customer.User.LastName).ToLower().Contains(s) ||
                    r.RentalId.ToString().Contains(s));
            }

            // Sorting
            query = queryParams.SortField?.ToLower() switch
            {
                "rentaldate" => queryParams.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(r => r.RentalDate)
                    : query.OrderBy(r => r.RentalDate),
                "returndate" => queryParams.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(r => r.ReturnDate)
                    : query.OrderBy(r => r.ReturnDate),
                "filmtitle" => queryParams.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(r => r.Inventory.Film.Title)
                    : query.OrderBy(r => r.Inventory.Film.Title),
                _ => query.OrderByDescending(r => r.RentalId)
            };

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / queryParams.PageSize);

            var data = await query
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .Select(r => new RentalResponseDto
                {
                    RentalId = r.RentalId,
                    RentalDate = r.RentalDate,
                    ReturnDate = r.ReturnDate,
                    InventoryId = r.InventoryId,
                    FilmId = r.Inventory.FilmId,
                    FilmTitle = r.Inventory.Film.Title,
                    CustomerId = r.CustomerId,
                    CustomerName = r.Customer.User.FirstName + " " + r.Customer.User.LastName,
                    StaffId = r.StaffId,
                    StaffName = r.Staff.User.FirstName + " " + r.Staff.User.LastName,
                    LastUpdate = r.LastUpdate,
                })
                .ToListAsync();

            return new PaginatedResponseDto<RentalResponseDto>
            {
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = queryParams.Page,
                PageSize = queryParams.PageSize,
                Data = data
            };
        }

        public async Task<RentalDetailDto?> GetRentalByIdAsync(int id)
        {
            var rental = await _rentalRepository.GetRentalByIdAsync(id);
            if (rental == null) return null;
            return MapToDetail(rental);
        }

        public async Task<RentalResponseDto> CreateRentalAsync(CreateRentalDto dto)
        {
            var rental = new Rental
            {
                InventoryId = dto.InventoryId,
                CustomerId = dto.CustomerId,
                StaffId = dto.StaffId,
                RentalDate = DateTime.UtcNow,
                LastUpdate = DateTime.UtcNow,
            };

            var created = await _rentalRepository.CreateRentalAsync(rental);
            return MapToResponse(created);
        }

        public async Task<RentalResponseDto?> ReturnRentalAsync(int rentalId)
        {
            var rental = await _rentalRepository.ReturnRentalAsync(rentalId);
            if (rental == null) return null;
            return MapToResponse(rental);
        }
    }
}
