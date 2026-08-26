using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Inventory;
using MovieRental.Domain.QueryParameters;

namespace MovieRental.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<PaginatedResponseDto<InventoryResponseDto>> GetAllInventoryAsync(InventoryQueryParametersDto queryParams);
        Task<InventoryDetailDto?> GetInventoryByIdAsync(int id);
        Task<InventoryResponseDto> CreateInventoryAsync(CreateInventoryDto dto);
        Task<InventoryResponseDto?> UpdateInventoryAsync(UpdateInventoryDto dto);
        Task<bool> DeleteInventoryAsync(int id);
    }
}
