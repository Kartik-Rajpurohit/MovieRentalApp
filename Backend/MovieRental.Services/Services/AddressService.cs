using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Locations.Addresses;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Extensions;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services;

// Handles business logic for address management and location associations.
public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;

    // Receives the address repository needed to manage address records.
    public AddressService(IAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    // Retrieves addresses using the specified filtering, searching, sorting, and pagination options.
    public async Task<PaginatedResponseDto<AddressResponseDto>> GetAllAddressesAsync(
        PaginationInputDto pagination,
        AddressFilterDto filter)
    {
        // Get the base query from the repository.
        var query = _addressRepository.GetAllAddresses();

        // 1. General search across street and city name.
        if (!string.IsNullOrWhiteSpace(pagination.Search))
        {
            var s = pagination.Search.Trim().ToLower();
            query = query.Where(a =>
                a.Street.ToLower().Contains(s) ||
                a.City.Name.ToLower().Contains(s));
        }

        // 2. Module Filters
        if (filter.CityId.HasValue)
            query = query.Where(a => a.CityId == filter.CityId.Value);

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            var city = filter.City.Trim().ToLower();
            query = query.Where(a => a.City.Name.ToLower().Contains(city));
        }

        if (!string.IsNullOrWhiteSpace(filter.PostalCode))
        {
            var postal = filter.PostalCode.Trim();
            query = query.Where(a => a.PostalCode != null && a.PostalCode.Contains(postal));
        }

        // 3. Dynamic Sorting
        var isDesc = string.Equals(pagination.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        query = pagination.SortBy?.ToLower() switch
        {
            "street" => isDesc
                ? query.OrderByDescending(a => a.Street)
                : query.OrderBy(a => a.Street),
            "city" => isDesc
                ? query.OrderByDescending(a => a.City.Name)
                : query.OrderBy(a => a.City.Name),
            "postalcode" => isDesc
                ? query.OrderByDescending(a => a.PostalCode)
                : query.OrderBy(a => a.PostalCode),
            "id" or "addressid" => isDesc
                ? query.OrderByDescending(a => a.AddressId)
                : query.OrderBy(a => a.AddressId),
            _ => isDesc
                ? query.OrderByDescending(a => a.AddressId)
                : query.OrderBy(a => a.AddressId)
        };

        // 4. Count
        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalRecords / pagination.PageSize);

        // 5. Pagination & Projection
        var data = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(a => new AddressResponseDto
            {
                AddressId = a.AddressId,
                Street = a.Street,
                PostalCode = a.PostalCode,
                Phone = a.Phone,
                CityId = a.CityId,
                CityName = a.City.Name,
                CountryName = a.City.Country.Name,
                LastUpdate = a.LastUpdate,
            })
            .ToListAsync();

        return new PaginatedResponseDto<AddressResponseDto>
        {
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            CurrentPage = pagination.Page,
            PageSize = pagination.PageSize,
            Data = data
        };
    }

    // Retrieves full address details by ID including related user and store counts.
    public async Task<AddressDetailDto?> GetAddressByIdAsync(int id)
    {
        var a = await _addressRepository.GetAddressByIdAsync(id);
        if (a == null) return null;

        // Convert the database entity into the detailed response DTO.
        return a.ToDetailDto();
    }

    // Validates request data and creates a new address record.
    public async Task<AddressResponseDto> CreateAddressAsync(CreateAddressDto dto)
    {
        if (!await _addressRepository.CityExistsAsync(dto.CityId))
        {
            throw new InvalidOperationException($"City with ID {dto.CityId} does not exist or has been deleted.");
        }

        // Map request DTO to database entity.
        var address = new Address
        {
            Street = dto.Street,
            PostalCode = dto.PostalCode,
            Phone = dto.Phone,
            CityId = dto.CityId,
            LastUpdate = DateTime.UtcNow,
        };

        // Persist the new address in the database.
        var created = await _addressRepository.CreateAddressAsync(address);

        // Map the created entity to the response DTO.
        return created.ToResponseDto();
    }
}