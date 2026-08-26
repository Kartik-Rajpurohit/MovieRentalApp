using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Payments;
using MovieRental.Domain.Entities;
using MovieRental.Domain.QueryParameters;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
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

            // Filters
            if (queryParams.CustomerId.HasValue)
                query = query.Where(p => p.CustomerId == queryParams.CustomerId.Value);

            if (queryParams.StaffId.HasValue)
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
            return MapToDetail(payment);
        }

        public async Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto dto)
        {
            var payment = new Payment
            {
                CustomerId = dto.CustomerId,
                StaffId = dto.StaffId,
                RentalId = dto.RentalId,
                Amount = dto.Amount,
                PaymentDate = DateTime.UtcNow,
            };

            var created = await _paymentRepository.CreatePaymentAsync(payment);
            return MapToResponse(created);
        }
    }
}
