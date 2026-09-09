using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Stores;
using MovieRental.Domain.QueryParameters;

namespace MovieRental.Services.Interfaces
{
    // Defines business operations for physical store locations and their manager assignments.
    public interface IStoreService
    {
        // Retrieves a paginated and filtered list of stores.
        Task<PaginatedResponseDto<StoreResponseDto>> GetAllStoresAsync(StoreQueryParametersDto queryParams);

        // Retrieves detailed store information by ID, including address, manager, and staff count.
        Task<StoreDetailDto?> GetStoreByIdAsync(int id);

        // Creates a new store record with an address and assigned manager.
        Task<StoreResponseDto> CreateStoreAsync(CreateStoreDto dto);
    }
}
