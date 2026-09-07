using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Rentals;
using MovieRental.Domain.Entities;
using MovieRental.Domain.QueryParameters;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;
using System.Security.Claims;

namespace MovieRental.Services.Services
{
    public class RentalService : IRentalService
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RentalService(IRentalRepository rentalRepository, IHttpContextAccessor httpContextAccessor)
        {
            _rentalRepository = rentalRepository;
            _httpContextAccessor = httpContextAccessor;
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

            var userPrincipal = _httpContextAccessor.HttpContext?.User;
            var role = userPrincipal?.FindFirst(ClaimTypes.Role)?.Value;

            // Automatic server-side role scoping
            if (role == "Customer")
            {
                var customerIdClaim = userPrincipal?.FindFirst("customerId")?.Value;
                if (int.TryParse(customerIdClaim, out var customerId))
                {
                    query = query.Where(r => r.CustomerId == customerId);
                }
                else
                {
                    return new PaginatedResponseDto<RentalResponseDto>
                    {
                        TotalRecords = 0,
                        TotalPages = 0,
                        CurrentPage = queryParams.Page,
                        PageSize = queryParams.PageSize,
                        Data = new List<RentalResponseDto>()
                    };
                }
            }
            else if (role == "Staff")
            {
                var storeIdClaim = userPrincipal?.FindFirst("storeId")?.Value;
                if (int.TryParse(storeIdClaim, out var storeId))
                {
                    query = query.Where(r => r.Staff.StoreId == storeId);
                }
            }
            else if (queryParams.CustomerId.HasValue)
            {
                query = query.Where(r => r.CustomerId == queryParams.CustomerId.Value);
            }

            if (role != "Staff" && queryParams.StaffId.HasValue)
                query = query.Where(r => r.StaffId == queryParams.StaffId.Value);

            if (queryParams.InventoryId.HasValue)
                query = query.Where(r => r.InventoryId == queryParams.InventoryId.Value);

            if (queryParams.IsReturned.HasValue)
                query = queryParams.IsReturned.Value
                    ? query.Where(r => r.ReturnDate != null)
                    : query.Where(r => r.ReturnDate == null);

            if (queryParams.HasPayment.HasValue)
                query = queryParams.HasPayment.Value
                    ? query.Where(r => r.Payments.Any())
                    : query.Where(r => !r.Payments.Any());

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
                    RentalRate = r.Inventory.Film.RentalRate,
                    SuggestedAmount = r.ReturnDate.HasValue
                        ? r.Inventory.Film.RentalRate *
                          (decimal)Math.Max(1, (r.ReturnDate.Value - r.RentalDate).TotalDays)
                        : r.Inventory.Film.RentalRate,
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

            var userPrincipal = _httpContextAccessor.HttpContext?.User;
            var role = userPrincipal?.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Customer")
            {
                var customerIdClaim = userPrincipal?.FindFirst("customerId")?.Value;
                if (!int.TryParse(customerIdClaim, out var customerId) || rental.CustomerId != customerId)
                    return null; // IDOR protection: Customer cannot view another's rental
            }
            else if (role == "Staff")
            {
                var storeIdClaim = userPrincipal?.FindFirst("storeId")?.Value;
                if (int.TryParse(storeIdClaim, out var storeId) && rental.Staff?.StoreId != storeId)
                    return null; // Staff can only view their store's rental
            }

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
