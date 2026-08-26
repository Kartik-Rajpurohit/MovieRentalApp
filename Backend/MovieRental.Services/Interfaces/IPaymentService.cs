using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Payments;
using MovieRental.Domain.QueryParameters;

namespace MovieRental.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaginatedResponseDto<PaymentResponseDto>> GetAllPaymentsAsync(PaymentQueryParametersDto queryParams);
        Task<PaymentDetailDto?> GetPaymentByIdAsync(int id);
        Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto dto);
    }
}
