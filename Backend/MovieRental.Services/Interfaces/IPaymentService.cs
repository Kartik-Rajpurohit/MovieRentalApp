using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Payments;
using MovieRental.Domain.QueryParameters;

namespace MovieRental.Services.Interfaces
{
    // Defines business operations for customer rental payments.
    public interface IPaymentService
    {
        // Retrieves a paginated and filtered history of payments.
        Task<PaginatedResponseDto<PaymentResponseDto>> GetAllPaymentsAsync(PaymentQueryParametersDto queryParams);

        // Retrieves detailed payment information by ID.
        Task<PaymentDetailDto?> GetPaymentByIdAsync(int id);

        // Validates and processes a new rental payment.
        Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto dto);
    }
}
