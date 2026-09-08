using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Payments;
using MovieRental.Domain.Entities;
using MovieRental.Domain.QueryParameters;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;
using System.Security.Claims;

using Microsoft.Extensions.Logging;

namespace MovieRental.Services.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IRentalRepository _rentalRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IRentalRepository rentalRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _rentalRepository = rentalRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private static PaymentResponseDto MapToResponse(Payment p) => new()
        {
            PaymentId = p.PaymentId,
            CustomerId = p.CustomerId,
            CustomerName = p.Customer?.User != null
                ? $"{p.Customer.User.FirstName} {p.Customer.User.LastName}".Trim()
                : $"Customer {p.CustomerId}",
            StaffId = p.StaffId,
            StaffName = p.Staff?.User != null
                ? $"{p.Staff.User.FirstName} {p.Staff.User.LastName}".Trim()
                : $"Staff {p.StaffId}",
            RentalId = p.RentalId,
            FilmTitle = p.Rental?.Inventory?.Film?.Title ?? "",
            Amount = p.Amount,
            PaymentDate = p.PaymentDate,
        };

        private static PaymentDetailDto MapToDetail(Payment p) => new()
        {
            PaymentId = p.PaymentId,
            CustomerId = p.CustomerId,
            CustomerName = p.Customer?.User != null
                ? $"{p.Customer.User.FirstName} {p.Customer.User.LastName}".Trim()
                : $"Customer {p.CustomerId}",
            StaffId = p.StaffId,
            StaffName = p.Staff?.User != null
                ? $"{p.Staff.User.FirstName} {p.Staff.User.LastName}".Trim()
                : $"Staff {p.StaffId}",
            RentalId = p.RentalId,
            FilmTitle = p.Rental?.Inventory?.Film?.Title ?? "",
            Amount = p.Amount,
            PaymentDate = p.PaymentDate,
        };

        public async Task<PaginatedResponseDto<PaymentResponseDto>> GetAllPaymentsAsync(PaymentQueryParametersDto queryParams)
        {
            var query = _paymentRepository.GetAllPayments();

            var userPrincipal = _httpContextAccessor.HttpContext?.User;
            var role = userPrincipal?.FindFirst(ClaimTypes.Role)?.Value;

            // Automatic server-side role scoping
            if (role == "Customer")
            {
                var customerIdClaim = userPrincipal?.FindFirst("customerId")?.Value;
                if (int.TryParse(customerIdClaim, out var customerId))
                {
                    query = query.Where(p => p.CustomerId == customerId);
                }
                else
                {
                    return new PaginatedResponseDto<PaymentResponseDto>
                    {
                        TotalRecords = 0,
                        TotalPages = 0,
                        CurrentPage = queryParams.Page,
                        PageSize = queryParams.PageSize,
                        Data = new List<PaymentResponseDto>()
                    };
                }
            }
            else if (role == "Staff")
            {
                var storeIdClaim = userPrincipal?.FindFirst("storeId")?.Value;
                if (int.TryParse(storeIdClaim, out var storeId))
                {
                    query = query.Where(p => p.Staff.StoreId == storeId);
                }
            }
            else if (queryParams.CustomerId.HasValue)
            {
                query = query.Where(p => p.CustomerId == queryParams.CustomerId.Value);
            }

            if (role != "Staff" && queryParams.StaffId.HasValue)
                query = query.Where(p => p.StaffId == queryParams.StaffId.Value);

            if (queryParams.RentalId.HasValue)
                query = query.Where(p => p.RentalId == queryParams.RentalId.Value);

            // Search
            if (!string.IsNullOrEmpty(queryParams.Search))
            {
                var s = queryParams.Search.ToLower();
                query = query.Where(p =>
                    p.Rental.Inventory.Film.Title.ToLower().Contains(s) ||
                    (p.Customer.User.FirstName + " " + p.Customer.User.LastName).ToLower().Contains(s) ||
                    p.PaymentId.ToString().Contains(s));
            }

            // Sorting
            query = queryParams.SortField?.ToLower() switch
            {
                "amount" => queryParams.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.Amount)
                    : query.OrderBy(p => p.Amount),
                "paymentdate" => queryParams.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.PaymentDate)
                    : query.OrderBy(p => p.PaymentDate),
                "filmtitle" => queryParams.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.Rental.Inventory.Film.Title)
                    : query.OrderBy(p => p.Rental.Inventory.Film.Title),
                _ => query.OrderByDescending(p => p.PaymentId)
            };

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / queryParams.PageSize);

            var data = await query
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .Select(p => new PaymentResponseDto
                {
                    PaymentId = p.PaymentId,
                    CustomerId = p.CustomerId,
                    CustomerName = p.Customer.User.FirstName + " " + p.Customer.User.LastName,
                    StaffId = p.StaffId,
                    StaffName = p.Staff.User.FirstName + " " + p.Staff.User.LastName,
                    RentalId = p.RentalId,
                    FilmTitle = p.Rental.Inventory.Film.Title,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                })
                .ToListAsync();

            return new PaginatedResponseDto<PaymentResponseDto>
            {
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = queryParams.Page,
                PageSize = queryParams.PageSize,
                Data = data
            };
        }

        public async Task<PaymentDetailDto?> GetPaymentByIdAsync(int id)
        {
            var payment = await _paymentRepository.GetPaymentByIdAsync(id);
            if (payment == null) return null;

            var userPrincipal = _httpContextAccessor.HttpContext?.User;
            var role = userPrincipal?.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Customer")
            {
                var customerIdClaim = userPrincipal?.FindFirst("customerId")?.Value;
                if (!int.TryParse(customerIdClaim, out var customerId) || payment.CustomerId != customerId)
                    return null; // IDOR protection: Customer cannot view another's payment
            }
            else if (role == "Staff")
            {
                var storeIdClaim = userPrincipal?.FindFirst("storeId")?.Value;
                if (int.TryParse(storeIdClaim, out var storeId) && payment.Staff?.StoreId != storeId)
                    return null; // Staff can only view their store's payment
            }

            return MapToDetail(payment);
        }

        public async Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto dto)
        {
            if (dto.Amount <= 0)
            {
                _logger.LogWarning("Payment creation rejected: non-positive amount {Amount}", dto.Amount);
                throw new InvalidOperationException("Payment amount must be greater than zero.");
            }

            var rental = await _rentalRepository.GetRentalByIdAsync(dto.RentalId);
            if (rental == null)
            {
                _logger.LogWarning("Payment creation rejected: Rental #{RentalId} does not exist", dto.RentalId);
                throw new InvalidOperationException($"Rental #{dto.RentalId} does not exist.");
            }

            if (rental.CustomerId != dto.CustomerId)
            {
                _logger.LogWarning("Payment creation rejected: Customer #{CustomerId} does not match Rental #{RentalId} customer #{RentalCustomerId}",
                    dto.CustomerId, dto.RentalId, rental.CustomerId);
                throw new InvalidOperationException(
                    $"Customer #{dto.CustomerId} does not match the customer on rental record #{dto.RentalId} (Customer #{rental.CustomerId}).");
            }

            var userPrincipal = _httpContextAccessor.HttpContext?.User;
            var role = userPrincipal?.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Staff")
            {
                var storeIdClaim = userPrincipal?.FindFirst("storeId")?.Value;
                if (int.TryParse(storeIdClaim, out var storeId))
                {
                    if (rental.Inventory?.StoreId != storeId && rental.Staff?.StoreId != storeId)
                    {
                        _logger.LogWarning("Payment creation rejected: Staff store {StoreId} does not match Rental #{RentalId}",
                            storeId, dto.RentalId);
                        throw new InvalidOperationException("Staff can only process payments for rentals belonging to their assigned store.");
                    }
                }

                var staffIdClaim = userPrincipal?.FindFirst("staffId")?.Value;
                if (int.TryParse(staffIdClaim, out var callerStaffId))
                {
                    dto.StaffId = callerStaffId; // Enforce caller identity to prevent staff impersonation
                }
            }

            var payment = new Payment
            {
                CustomerId = dto.CustomerId,
                StaffId = dto.StaffId,
                RentalId = dto.RentalId,
                Amount = dto.Amount,
                PaymentDate = DateTime.UtcNow,
            };

            var created = await _paymentRepository.CreatePaymentAsync(payment);

            _logger.LogInformation("Payment created successfully: PaymentId #{PaymentId}, Amount: {Amount}, RentalId #{RentalId}, CustomerId #{CustomerId}, StaffId #{StaffId}",
                created.PaymentId, created.Amount, created.RentalId, created.CustomerId, created.StaffId);

            return MapToResponse(created);
        }
    }
}
