using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Inventory;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Extensions;
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

        // Gets paginated inventory copies with store, movie, and availability filters.
        public async Task<PaginatedResponseDto<InventoryResponseDto>> GetAllInventoryAsync(
            PaginationInputDto pagination,
            InventoryFilterDto filter)
        {
            var query = _inventoryRepository.GetAllInventory();

            // 1. Search — by movie title or inventory ID
            if (!string.IsNullOrWhiteSpace(pagination.Search))
            {
                var s = pagination.Search.Trim().ToLower();
                query = query.Where(i =>
                    i.Movie.Title.ToLower().Contains(s) ||
                    i.InventoryId.ToString().Contains(s));
            }

            // 2. Module Filters
            if (filter.MovieId.HasValue)
                query = query.Where(i => i.MovieId == filter.MovieId.Value);

            if (filter.StoreId.HasValue)
                query = query.Where(i => i.StoreId == filter.StoreId.Value);

            if (filter.IsAvailable.HasValue)
            {
                query = filter.IsAvailable.Value
                    ? query.Where(i => !i.Rentals.Any(r => r.ReturnDate == null))
                    : query.Where(i => i.Rentals.Any(r => r.ReturnDate == null));
            }

            // 3. Sorting
            var isDesc = string.Equals(pagination.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
            query = pagination.SortBy?.ToLower() switch
            {
                "movietitle" or "movie" => isDesc
                    ? query.OrderByDescending(i => i.Movie.Title)
                    : query.OrderBy(i => i.Movie.Title),
                "storeid" or "store" => isDesc
                    ? query.OrderByDescending(i => i.StoreId)
                    : query.OrderBy(i => i.StoreId),
                "id" or "inventoryid" => isDesc
                    ? query.OrderByDescending(i => i.InventoryId)
                    : query.OrderBy(i => i.InventoryId),
                _ => isDesc
                    ? query.OrderByDescending(i => i.InventoryId)
                    : query.OrderBy(i => i.InventoryId)
            };

            // 4. Count
            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pagination.PageSize);

            // 5. Pagination & Materialization
            var entities = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            return new PaginatedResponseDto<InventoryResponseDto>
            {
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = pagination.Page,
                PageSize = pagination.PageSize,
                Data = entities.Select(i => i.ToResponseDto()).ToList()
            };
        }

        // Gets inventory details including total rental count.
        public async Task<InventoryDetailDto?> GetInventoryByIdAsync(int id)
        {
            var inventory = await _inventoryRepository.GetInventoryByIdAsync(id);
            if (inventory == null) return null;
            return inventory.ToDetailDto();
        }

        // Adds a new movie inventory copy to a store.
        public async Task<InventoryResponseDto> CreateInventoryAsync(CreateInventoryDto dto)
        {
            if (!await _inventoryRepository.MovieExistsAsync(dto.MovieId))
            {
                throw new InvalidOperationException($"Movie with ID {dto.MovieId} does not exist or has been deleted.");
            }

            if (!await _inventoryRepository.StoreExistsAsync(dto.StoreId))
            {
                throw new InvalidOperationException($"Store with ID {dto.StoreId} does not exist or has been deleted.");
            }

            var entity = new Inventory
            {
                MovieId = dto.MovieId,
                StoreId = dto.StoreId,
                LastUpdate = DateTime.UtcNow
            };
            var inventory = await _inventoryRepository.CreateInventoryAsync(entity);
            return inventory.ToResponseDto();
        }

        // Updates the store assignment of an inventory copy.
        public async Task<InventoryResponseDto?> UpdateInventoryAsync(UpdateInventoryDto dto)
        {
            if (dto.StoreId.HasValue && !await _inventoryRepository.StoreExistsAsync(dto.StoreId.Value))
            {
                throw new InvalidOperationException($"Store with ID {dto.StoreId.Value} does not exist or has been deleted.");
            }

            var entity = new Inventory
            {
                InventoryId = dto.InventoryId,
                StoreId = dto.StoreId ?? 0,
                LastUpdate = DateTime.UtcNow
            };
            var inventory = await _inventoryRepository.UpdateInventoryAsync(entity);
            if (inventory == null) return null;
            return inventory.ToResponseDto();
        }

        // Deletes an inventory copy record by ID through repository.
        public async Task<bool> DeleteInventoryAsync(int id)
            => await _inventoryRepository.DeleteInventoryAsync(id);
    }
}
