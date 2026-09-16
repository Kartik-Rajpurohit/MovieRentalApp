using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Stores;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    // Handles business logic for store management, manager assignments, and store metrics.
    public class StoreService : IStoreService
    {
        private readonly IStoreRepository _storeRepository;

        // Receives the store repository needed for store operations.
        public StoreService(IStoreRepository storeRepository)
        {
            _storeRepository = storeRepository;
        }

        // Retrieves a paginated and filtered list of stores with aggregated counts.
        public async Task<PaginatedResponseDto<StoreResponseDto>> GetAllStoresAsync(
            PaginationInputDto pagination,
            StoreFilterDto filter)
        {
            // Get the base query from the repository.
            var query = _storeRepository.GetAllStores();

            // 1. Search by store ID, city, country, or manager name.
            if (!string.IsNullOrWhiteSpace(pagination.Search))
            {
                var s = pagination.Search.Trim().ToLower();
                query = query.Where(st =>
                    st.StoreId.ToString().Contains(s) ||
                    st.Address.City.Name.ToLower().Contains(s) ||
                    st.Address.City.Country.Name.ToLower().Contains(s) ||
                    (st.ManagerStaff != null && st.ManagerStaff.User != null
                        ? (st.ManagerStaff.User.FirstName + " " + st.ManagerStaff.User.LastName).ToLower()
                        : "").Contains(s));
            }

            // 2. Module Filters
            if (!string.IsNullOrWhiteSpace(filter.City))
            {
                var city = filter.City.Trim().ToLower();
                query = query.Where(s => s.Address.City.Name.ToLower().Contains(city));
            }

            if (!string.IsNullOrWhiteSpace(filter.Country))
            {
                var country = filter.Country.Trim().ToLower();
                query = query.Where(s => s.Address.City.Country.Name.ToLower().Contains(country));
            }

            // 3. Dynamic Sorting
            var isDesc = string.Equals(pagination.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
            query = pagination.SortBy?.ToLower() switch
            {
                "city" => isDesc
                    ? query.OrderByDescending(s => s.Address.City.Name)
                    : query.OrderBy(s => s.Address.City.Name),
                "country" => isDesc
                    ? query.OrderByDescending(s => s.Address.City.Country.Name)
                    : query.OrderBy(s => s.Address.City.Country.Name),
                "id" or "storeid" => isDesc
                    ? query.OrderByDescending(s => s.StoreId)
                    : query.OrderBy(s => s.StoreId),
                _ => isDesc
                    ? query.OrderByDescending(s => s.StoreId)
                    : query.OrderBy(s => s.StoreId)
            };

            // 4. Count
            var totalRecords = await query.CountAsync();
            var totalPages   = (int)Math.Ceiling((double)totalRecords / pagination.PageSize);

            // 5. Pagination & Projection
            var data = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(s => new StoreResponseDto
                {
                    StoreId        = s.StoreId,
                    ManagerStaffId = s.ManagerStaffId,
                    ManagerName    = s.ManagerStaff != null && s.ManagerStaff.User != null
                        ? (s.ManagerStaff.User.FirstName + " " + s.ManagerStaff.User.LastName).Trim()
                        : "Staff #" + s.ManagerStaffId,
                    Street      = s.Address != null ? s.Address.Street ?? "" : "",
                    PostalCode  = s.Address != null ? s.Address.PostalCode : null,
                    Phone       = s.Address != null ? s.Address.Phone ?? "" : "",
                    CityName    = s.Address != null && s.Address.City != null ? s.Address.City.Name : "",
                    CountryName = s.Address != null && s.Address.City != null && s.Address.City.Country != null
                        ? s.Address.City.Country.Name : "",
                    TotalStaff     = s.Staff.Count(),
                    TotalCustomers = s.Customers.Count(),
                    TotalInventory = s.Inventories.Count(),
                })
                .ToListAsync();

            return new PaginatedResponseDto<StoreResponseDto>
            {
                TotalRecords = totalRecords,
                TotalPages   = totalPages,
                CurrentPage  = pagination.Page,
                PageSize     = pagination.PageSize,
                Data         = data
            };
        }

        // Retrieves detailed store information by ID including address, manager, and operational counts.
        public async Task<StoreDetailDto?> GetStoreByIdAsync(int id)
        {
            return await _storeRepository.GetAllStores()
                .Where(s => s.StoreId == id)
                .Select(s => new StoreDetailDto
                {
                    StoreId        = s.StoreId,
                    ManagerStaffId = s.ManagerStaffId,
                    ManagerName    = s.ManagerStaff != null && s.ManagerStaff.User != null
                        ? (s.ManagerStaff.User.FirstName + " " + s.ManagerStaff.User.LastName).Trim()
                        : "Staff #" + s.ManagerStaffId,
                    AddressId   = s.AddressId,
                    Street      = s.Address != null ? s.Address.Street ?? "" : "",
                    PostalCode  = s.Address != null ? s.Address.PostalCode : null,
                    Phone       = s.Address != null ? s.Address.Phone ?? "" : "",
                    CityName    = s.Address != null && s.Address.City != null ? s.Address.City.Name : "",
                    CountryName = s.Address != null && s.Address.City != null && s.Address.City.Country != null
                        ? s.Address.City.Country.Name : "",
                    TotalStaff     = s.Staff.Count(),
                    TotalCustomers = s.Customers.Count(),
                    TotalInventory = s.Inventories.Count(),
                    LastUpdate     = s.LastUpdate,
                })
                .FirstOrDefaultAsync();
        }

        // Validates and creates a new store location record.
        public async Task<StoreResponseDto> CreateStoreAsync(CreateStoreDto dto)
        {
            var store = new Store
            {
                ManagerStaffId = dto.ManagerStaffId ?? 0,
                AddressId = dto.AddressId,
                LastUpdate = DateTime.UtcNow,
            };
            var created = await _storeRepository.CreateStoreAsync(store);
            return MapToDto(created);
        }

        // Maps raw Store entity to StoreResponseDto for in-memory created results.
        private static StoreResponseDto MapToDto(Store s) => new StoreResponseDto
        {
            StoreId        = s.StoreId,
            ManagerStaffId = s.ManagerStaffId,
            ManagerName    = s.ManagerStaff?.User != null
                ? (s.ManagerStaff.User.FirstName + " " + s.ManagerStaff.User.LastName).Trim()
                : "Unassigned",
            Street      = s.Address?.Street ?? "",
            PostalCode  = s.Address?.PostalCode,
            Phone       = s.Address?.Phone ?? "",
            CityName    = s.Address?.City?.Name ?? "",
            CountryName = s.Address?.City?.Country?.Name ?? "",
            TotalStaff     = 0,
            TotalCustomers = 0,
            TotalInventory = 0,
        };
    }
}
