using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Inventory;
using MovieRental.Domain.Entities;
using MovieRental.Domain.QueryParameters;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    // Handles business logic for inventory copy queries, availability evaluation, and store updates.
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        // Receives the inventory repository needed to access inventory records.
        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        // Maps raw entity to response DTO — item is marked available if it has no active unreturned rentals.
        private static InventoryResponseDto MapToResponse(Inventory i) => new()
        {
            InventoryId = i.InventoryId,
            FilmId = i.FilmId,
            FilmTitle = i.Film?.Title ?? "",
            StoreId = i.StoreId,
            IsAvailable = !i.Rentals.Any(r => r.ReturnDate == null),
            LastUpdate = i.LastUpdate,
        };

        // Maps raw entity to detail DTO with lifetime rental count and availability status.
        private static InventoryDetailDto MapToDetail(Inventory i) => new()
        {
            InventoryId = i.InventoryId,
            FilmId = i.FilmId,
            FilmTitle = i.Film?.Title ?? "",
            StoreId = i.StoreId,
            IsAvailable = !i.Rentals.Any(r => r.ReturnDate == null),
            TotalRentals = i.Rentals.Count,
            LastUpdate = i.LastUpdate,
        };

        // Gets paginated inventory copies with store, film, and availability filters.
        public async Task<PaginatedResponseDto<InventoryResponseDto>> GetAllInventoryAsync(
            InventoryQueryParametersDto queryParams)
        {
            var query = _inventoryRepository.GetAllInventory();

            // Filter by film
            if (queryParams.FilmId.HasValue)
                query = query.Where(i => i.FilmId == queryParams.FilmId.Value);

            // Filter by store
            if (queryParams.StoreId.HasValue)
                query = query.Where(i => i.StoreId == queryParams.StoreId.Value);

            // Filter by availability in database query before pagination
            if (queryParams.IsAvailable.HasValue)
            {
                query = queryParams.IsAvailable.Value
                    ? query.Where(i => !i.Rentals.Any(r => r.ReturnDate == null))
                    : query.Where(i => i.Rentals.Any(r => r.ReturnDate == null));
            }

            // Global search — by film title or inventory ID
            if (!string.IsNullOrEmpty(queryParams.Search))
            {
                var s = queryParams.Search.ToLower();
                query = query.Where(i =>
                    i.Film.Title.ToLower().Contains(s) ||
                    i.InventoryId.ToString().Contains(s));
            }

            // Sorting
            query = queryParams.SortField?.ToLower() switch
            {
                "filmtitle" => queryParams.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(i => i.Film.Title)
                    : query.OrderBy(i => i.Film.Title),
                "storeid" => queryParams.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(i => i.StoreId)
                    : query.OrderBy(i => i.StoreId),
                _ => query.OrderBy(i => i.InventoryId)
            };

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / queryParams.PageSize);

            var entities = await query
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            return new PaginatedResponseDto<InventoryResponseDto>
            {
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = queryParams.Page,
                PageSize = queryParams.PageSize,
                Data = entities.Select(MapToResponse).ToList()
            };
        }

        // Gets inventory details including total rental count.
        public async Task<InventoryDetailDto?> GetInventoryByIdAsync(int id)
        {
            var inventory = await _inventoryRepository.GetInventoryByIdAsync(id);
            if (inventory == null) return null;
            return MapToDetail(inventory);
        }

        // Adds a new movie inventory copy to a store.
        public async Task<InventoryResponseDto> CreateInventoryAsync(CreateInventoryDto dto)
        {
            var entity = new Inventory
            {
                FilmId = dto.FilmId,
                StoreId = dto.StoreId,
                LastUpdate = DateTime.UtcNow
            };
            var inventory = await _inventoryRepository.CreateInventoryAsync(entity);
            return MapToResponse(inventory);
        }

        // Updates the store assignment of an inventory copy.
        public async Task<InventoryResponseDto?> UpdateInventoryAsync(UpdateInventoryDto dto)
        {
            var entity = new Inventory
            {
                InventoryId = dto.InventoryId,
                StoreId = dto.StoreId ?? 0,
                LastUpdate = DateTime.UtcNow
            };
            var inventory = await _inventoryRepository.UpdateInventoryAsync(entity);
            if (inventory == null) return null;
            return MapToResponse(inventory);
        }

        // Deletes an inventory copy record by ID through repository.
        public async Task<bool> DeleteInventoryAsync(int id)
            => await _inventoryRepository.DeleteInventoryAsync(id);
    }
}
