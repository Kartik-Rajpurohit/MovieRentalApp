using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Stores;
using MovieRental.Domain.Entities;
using MovieRental.Domain.QueryParameters;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    public class StoreService : IStoreService
    {
        private readonly IStoreRepository _storeRepository;

        public StoreService(IStoreRepository storeRepository)
        {
            _storeRepository = storeRepository;
        }

        public async Task<PaginatedResponseDto<StoreResponseDto>> GetAllStoresAsync(StoreQueryParametersDto queryParams)
        {
            var query = _storeRepository.GetAllStores();

            if (!string.IsNullOrWhiteSpace(queryParams.City))
                query = query.Where(s =>
                    s.Address.City.Name.ToLower().Contains(queryParams.City.ToLower()));

            if (!string.IsNullOrWhiteSpace(queryParams.Country))
                query = query.Where(s =>
                    s.Address.City.Country.Name.ToLower().Contains(queryParams.Country.ToLower()));

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
                    District    = s.Address != null ? s.Address.District ?? "" : "",
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
                    District    = s.Address != null ? s.Address.District ?? "" : "",
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
        public async Task<StoreResponseDto> CreateStoreAsync(CreateStoreDto dto)
        {
            var store = new Store
            {
                ManagerStaffId = dto.ManagerStaffId,
                AddressId = dto.AddressId,
                LastUpdate = DateTime.UtcNow,
            };
            var created = await _storeRepository.CreateStoreAsync(store);
            return MapToDto(created);
        }

        // Private helper - maps Store entity to StoreResponseDto (same pattern as UserService, InventoryService)
        // Used only for in-memory mapping after Create; GetAll/GetById use EF .Select() for DB-side count aggregation
        private static StoreResponseDto MapToDto(Store s) => new StoreResponseDto
        {
            StoreId        = s.StoreId,
            ManagerStaffId = s.ManagerStaffId,
            ManagerName    = s.ManagerStaff?.User != null
                ? (s.ManagerStaff.User.FirstName + " " + s.ManagerStaff.User.LastName).Trim()
                : "Unassigned",
            Street      = s.Address?.Street ?? "",
            District    = s.Address?.District ?? "",
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
