using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Payments;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;
using System.Security.Claims;

using Microsoft.Extensions.Logging;

namespace MovieRental.Services.Services
{
    // Handles business logic for payment records, customer/staff role scoping, and transaction validation.
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IRentalRepository _rentalRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PaymentService> _logger;

        // Receives payment and rental repositories, HTTP context accessor for user claims, and audit logger.
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

        // Converts raw Payment entity into standard response DTO with formatted names.
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
            MovieTitle = p.Rental?.Inventory?.Movie?.Title ?? "",
            Amount = p.Amount,
            PaymentDate = p.PaymentDate,
        };


        // Gets a paginated list of payments with amount, date filters, and role-based data scoping.
        public async Task<PaginatedResponseDto<PaymentResponseDto>> GetAllPaymentsAsync(
            PaginationInputDto pagination,
            PaymentFilterDto filter)
        {
            var query = _paymentRepository.GetAllPayments();

            // Extract caller identity and role from current user JWT claims.
            var userPrincipal = _httpContextAccessor.HttpContext?.User;
            var role = userPrincipal?.FindFirst(ClaimTypes.Role)?.Value;

            // Enforce automatic server-side role scoping: Customers only see their own payments.
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
                        CurrentPage = pagination.Page,
                        PageSize = pagination.PageSize,
                        Data = new List<PaymentResponseDto>()
                    };
                }
            }
            // Staff members only see payments processed at their assigned store.
            else if (role == "Staff")
            {
                var storeIdClaim = userPrincipal?.FindFirst("storeId")?.Value;
                if (int.TryParse(storeIdClaim, out var storeId))
                {
                    query = query.Where(p => p.Staff.StoreId == storeId);
                }
            }
            else if (filter.CustomerId.HasValue)
            {
                query = query.Where(p => p.CustomerId == filter.CustomerId.Value);
            }

            // 1. Search across movie title, customer full name, or payment ID.
            if (!string.IsNullOrWhiteSpace(pagination.Search))
            {
                var s = pagination.Search.Trim().ToLower();
                query = query.Where(p =>
                    p.Rental.Inventory.Movie.Title.ToLower().Contains(s) ||
                    (p.Customer.User != null && (p.Customer.User.FirstName + " " + p.Customer.User.LastName).ToLower().Contains(s)) ||
                    p.PaymentId.ToString().Contains(s));
            }

            // 2. Module Filters
            if (role != "Staff" && filter.StaffId.HasValue)
                query = query.Where(p => p.StaffId == filter.StaffId.Value);

            if (filter.RentalId.HasValue)
                query = query.Where(p => p.RentalId == filter.RentalId.Value);

            // Amount range filters.
            if (filter.MinAmount.HasValue)
                query = query.Where(p => p.Amount >= filter.MinAmount.Value);

            if (filter.MaxAmount.HasValue)
                query = query.Where(p => p.Amount <= filter.MaxAmount.Value);

            // Date range filters in UTC.
            if (filter.FromDate.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(filter.FromDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(p => p.PaymentDate >= fromUtc);
            }

            if (filter.ToDate.HasValue)
            {
                var toUtc = DateTime.SpecifyKind(filter.ToDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                query = query.Where(p => p.PaymentDate <= toUtc);
            }

            // 3. Dynamic Sorting
            var isDesc = string.Equals(pagination.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
            query = pagination.SortBy?.ToLower() switch
            {
                "amount" => isDesc
                    ? query.OrderByDescending(p => p.Amount)
                    : query.OrderBy(p => p.Amount),
                "paymentdate" => isDesc
                    ? query.OrderByDescending(p => p.PaymentDate)
                    : query.OrderBy(p => p.PaymentDate),
                "movietitle" or "movie" => isDesc
                    ? query.OrderByDescending(p => p.Rental.Inventory.Movie.Title)
                    : query.OrderBy(p => p.Rental.Inventory.Movie.Title),
                "id" or "paymentid" => isDesc
                    ? query.OrderByDescending(p => p.PaymentId)
                    : query.OrderBy(p => p.PaymentId),
                _ => isDesc
                    ? query.OrderByDescending(p => p.PaymentId)
                    : query.OrderBy(p => p.PaymentId)
            };

            // 4. Count
            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pagination.PageSize);

            // 5. Pagination & Projection
            var data = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(p => new PaymentResponseDto
                {
                    PaymentId = p.PaymentId,
                    CustomerId = p.CustomerId,
                    CustomerName = p.Customer.User != null
                        ? (p.Customer.User.FirstName + " " + p.Customer.User.LastName).Trim()
                        : "Customer #" + p.CustomerId,
                    StaffId = p.StaffId,
                    StaffName = p.Staff.User != null
                        ? (p.Staff.User.FirstName + " " + p.Staff.User.LastName).Trim()
                        : "Staff #" + p.StaffId,
                    RentalId = p.RentalId,
                    MovieTitle = p.Rental.Inventory.Movie.Title,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                })
                .ToListAsync();

            return new PaginatedResponseDto<PaymentResponseDto>
            {
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = pagination.Page,
                PageSize = pagination.PageSize,
                Data = data
            };
        }

        // Gets payment details by ID, enforcing IDOR ownership checks for customers and staff.
        public async Task<PaymentResponseDto?> GetPaymentByIdAsync(int id)
        {
            var payment = await _paymentRepository.GetPaymentByIdAsync(id);
            if (payment == null) return null;

            var userPrincipal = _httpContextAccessor.HttpContext?.User;
            var role = userPrincipal?.FindFirst(ClaimTypes.Role)?.Value;

            // Prevent customers from inspecting other users' payments.
            if (role == "Customer")
            {
                var customerIdClaim = userPrincipal?.FindFirst("customerId")?.Value;
                if (!int.TryParse(customerIdClaim, out var customerId) || payment.CustomerId != customerId)
                    return null;
            }
            // Prevent staff from viewing payments outside their assigned store.
            else if (role == "Staff")
            {
                var storeIdClaim = userPrincipal?.FindFirst("storeId")?.Value;
                if (int.TryParse(storeIdClaim, out var storeId) && payment.Staff?.StoreId != storeId)
                    return null;
            }

            return MapToResponse(payment);
        }

        // Validates payment amount, rental existence, and customer match before creating record.
        public async Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto dto)
        {
            // Business rule: Payment amount must be strictly greater than zero.
            if (dto.Amount <= 0)
            {
                _logger.LogWarning("Payment creation rejected: non-positive amount {Amount}", dto.Amount);
                throw new InvalidOperationException("Payment amount must be greater than zero.");
            }

            // Business rule: Rental must exist in the database.
            var rental = await _rentalRepository.GetRentalByIdAsync(dto.RentalId);
            if (rental == null)
            {
                _logger.LogWarning("Payment creation rejected: Rental #{RentalId} does not exist", dto.RentalId);
                throw new InvalidOperationException($"Rental #{dto.RentalId} does not exist.");
            }

            // Business rule: Customer paying must match the customer who rented the movie.
            if (rental.CustomerId != dto.CustomerId)
            {
                _logger.LogWarning("Payment creation rejected: Customer #{CustomerId} does not match Rental #{RentalId} customer #{RentalCustomerId}",
                    dto.CustomerId, dto.RentalId, rental.CustomerId);
                throw new InvalidOperationException(
                    $"Customer #{dto.CustomerId} does not match the customer on rental record #{dto.RentalId} (Customer #{rental.CustomerId}).");
            }

            if (!await _rentalRepository.CustomerExistsAsync(dto.CustomerId))
            {
                _logger.LogWarning("Payment creation rejected: Customer #{CustomerId} does not exist or has been deleted", dto.CustomerId);
                throw new InvalidOperationException($"Customer #{dto.CustomerId} does not exist or has been deleted.");
            }

            if (!await _rentalRepository.StaffExistsAsync(dto.StaffId))
            {
                _logger.LogWarning("Payment creation rejected: Staff member #{StaffId} does not exist or has been deleted", dto.StaffId);
                throw new InvalidOperationException($"Staff member #{dto.StaffId} does not exist or has been deleted.");
            }

            var userPrincipal = _httpContextAccessor.HttpContext?.User;
            var role = userPrincipal?.FindFirst(ClaimTypes.Role)?.Value;

            // Security check: Staff can only collect payments for rentals associated with their store.
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

                // Enforce caller identity to prevent staff impersonation.
                var staffIdClaim = userPrincipal?.FindFirst("staffId")?.Value;
                if (int.TryParse(staffIdClaim, out var callerStaffId))
                {
                    dto.StaffId = callerStaffId;
                }
            }

            // Map request DTO to database entity with current UTC timestamp.
            var payment = new Payment
            {
                CustomerId = dto.CustomerId,
                StaffId = dto.StaffId,
                RentalId = dto.RentalId,
                Amount = dto.Amount,
                PaymentDate = DateTime.UtcNow,
            };

            // Persist the payment record in the database.
            var created = await _paymentRepository.CreatePaymentAsync(payment);

            _logger.LogInformation("Payment created successfully: PaymentId #{PaymentId}, Amount: {Amount}, RentalId #{RentalId}, CustomerId #{CustomerId}, StaffId #{StaffId}",
                created.PaymentId, created.Amount, created.RentalId, created.CustomerId, created.StaffId);

            return MapToResponse(created);
        }
    }
}
