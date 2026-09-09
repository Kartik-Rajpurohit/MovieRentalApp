using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Rentals;
using MovieRental.Domain.QueryParameters;

namespace MovieRental.Services.Interfaces
{
    // Defines business operations for renting movie inventory and handling returns.
    public interface IRentalService
    {
        // Retrieves a paginated and filtered list of rental transactions.
        Task<PaginatedResponseDto<RentalResponseDto>> GetAllRentalsAsync(RentalQueryParametersDto queryParams);

        // Retrieves detailed rental information by ID, including payments and customer details.
        Task<RentalDetailDto?> GetRentalByIdAsync(int id);

        // Validates availability and creates a new rental transaction for an inventory item.
        Task<RentalResponseDto> CreateRentalAsync(CreateRentalDto dto);

        // Marks a movie rental as returned and updates inventory status.
        Task<RentalResponseDto?> ReturnRentalAsync(int rentalId);
    }
}
