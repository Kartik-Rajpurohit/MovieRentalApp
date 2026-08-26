using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Stores;
using MovieRental.Domain.QueryParameters;

namespace MovieRental.Services.Interfaces
{
    public interface IStoreService
    {
        Task<PaginatedResponseDto<StoreResponseDto>> GetAllStoresAsync(StoreQueryParametersDto queryParams);
        Task<StoreDetailDto?> GetStoreByIdAsync(int id);
    }
}
