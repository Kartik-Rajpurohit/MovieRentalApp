using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Inventory;
using MovieRental.Domain.Entities;
using MovieRental.Domain.QueryParameters;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        // Map raw entity to response DTO — availability = no active rental (return_date is null)
        private static InventoryResponseDto MapToResponse(Inventory i) => new()
        {
            InventoryId = i.InventoryId,
            FilmId = i.FilmId,
            FilmTitle = i.Film?.Title ?? "",
            StoreId = i.StoreId,
            IsAvailable = !i.Rentals.Any(r => r.ReturnDate == null),
            LastUpdate = i.LastUpdate,
        };

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

            // Fetch in memory — IsAvailable computed from Rentals collection
            var entities = await query
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            // Filter by availability after fetch (can't translate .Any on collection in EF)
            if (queryParams.IsAvailable.HasValue)
                entities = entities
                    .Where(i => MapToResponse(i).IsAvailable == queryParams.IsAvailable.Value)
                    .ToList();

            return new PaginatedResponseDto<InventoryResponseDto>
            {
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = queryParams.Page,
                PageSize = queryParams.PageSize,
                Data = entities.Select(MapToResponse).ToList()
            };
        }

        public async Task<InventoryDetailDto?> GetInventoryByIdAsync(int id)
        {
            var inventory = await _inventoryRepository.GetInventoryByIdAsync(id);
            if (inventory == null) return null;
            return MapToDetail(inventory);
        }

        public async Task<InventoryResponseDto> CreateInventoryAsync(CreateInventoryDto dto)
        {
            var inventory = await _inventoryRepository.CreateInventoryAsync(dto);
            return MapToResponse(inventory);
        }

        public async Task<InventoryResponseDto?> UpdateInventoryAsync(UpdateInventoryDto dto)
        {
            var inventory = await _inventoryRepository.UpdateInventoryAsync(dto);
            if (inventory == null) return null;
            return MapToResponse(inventory);
        }

        public async Task<bool> DeleteInventoryAsync(int id)
            => await _inventoryRepository.DeleteInventoryAsync(id);
    }
}
