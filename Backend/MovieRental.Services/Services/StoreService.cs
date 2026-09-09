using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Stores;
using MovieRental.Domain.Entities;
using MovieRental.Domain.QueryParameters;
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
        public async Task<PaginatedResponseDto<StoreResponseDto>> GetAllStoresAsync(StoreQueryParametersDto queryParams)
        {
            // Get the base query from the repository.
            var query = _storeRepository.GetAllStores();

            // Filter by city name if provided.
            if (!string.IsNullOrWhiteSpace(queryParams.City))
                query = query.Where(s =>
                    s.Address.City.Name.ToLower().Contains(queryParams.City.ToLower()));

            // Filter by country name if provided.
            if (!string.IsNullOrWhiteSpace(queryParams.Country))
                query = query.Where(s =>
                    s.Address.City.Country.Name.ToLower().Contains(queryParams.Country.ToLower()));

            // Search by store ID, city, country, or manager name.
            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                var s = queryParams.Search.ToLower();
                query = query.Where(st =>
                    st.StoreId.ToString().Contains(s) ||
                    st.Address.City.Name.ToLower().Contains(s) ||
                    st.Address.City.Country.Name.ToLower().Contains(s) ||
                    (st.ManagerStaff != null && st.ManagerStaff.User != null
                        ? (st.ManagerStaff.User.FirstName + " " + st.ManagerStaff.User.LastName).ToLower()
                        : "").Contains(s));
            }

            // Apply dynamic sorting.
            query = queryParams.SortField?.ToLower() switch
            {
                "storeid" => queryParams.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.StoreId)
                    : query.OrderBy(s => s.StoreId),
                "city" => queryParams.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.Address.City.Name)
                    : query.OrderBy(s => s.Address.City.Name),
                "country" => queryParams.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.Address.City.Country.Name)
                    : query.OrderBy(s => s.Address.City.Country.Name),
                _ => query.OrderBy(s => s.StoreId)
            };

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / queryParams.PageSize);

            // Paginate and project store entities to response DTOs with aggregated counts.
            var data = await query
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
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
                CurrentPage  = queryParams.Page,
                PageSize     = queryParams.PageSize,
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
