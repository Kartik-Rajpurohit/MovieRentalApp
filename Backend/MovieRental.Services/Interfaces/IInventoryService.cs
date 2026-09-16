using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Inventory;

namespace MovieRental.Services.Interfaces
{
    // Defines business operations for physical movie copies in store inventories.
    public interface IInventoryService
    {
        // Retrieves a paginated and filtered list of inventory items.
        Task<PaginatedResponseDto<InventoryResponseDto>> GetAllInventoryAsync(
            PaginationInputDto pagination,
            InventoryFilterDto filter);

        // Retrieves detailed information for a specific inventory copy.
        Task<InventoryDetailDto?> GetInventoryByIdAsync(int id);

        // Adds a new movie copy to a store's inventory.
        Task<InventoryResponseDto> CreateInventoryAsync(CreateInventoryDto dto);

        // Updates an inventory record.
        Task<InventoryResponseDto?> UpdateInventoryAsync(UpdateInventoryDto dto);

        // Removes an inventory item by ID.
        Task<bool> DeleteInventoryAsync(int id);
    }
}
