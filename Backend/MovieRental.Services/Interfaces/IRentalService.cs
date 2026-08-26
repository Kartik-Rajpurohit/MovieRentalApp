using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Rentals;
using MovieRental.Domain.QueryParameters;

namespace MovieRental.Services.Interfaces
{
    public interface IRentalService
    {
        Task<PaginatedResponseDto<RentalResponseDto>> GetAllRentalsAsync(RentalQueryParametersDto queryParams);
        Task<RentalDetailDto?> GetRentalByIdAsync(int id);
        Task<RentalResponseDto> CreateRentalAsync(CreateRentalDto dto);
        Task<RentalResponseDto?> ReturnRentalAsync(int rentalId);
    }
}
