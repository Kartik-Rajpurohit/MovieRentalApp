using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Locations.Addresses;
using MovieRental.Domain.Entities;
using MovieRental.Domain.QueryParameters;
using MovieRental.Repository.Interfaces;
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
    public async Task<PaginatedResponseDto<AddressResponseDto>> GetAllAddressesAsync(AddressQueryParametersDto queryParams)
    {
        // Get the base query from the repository.
        var query = _addressRepository.GetAllAddresses();

        // Filter by specific city ID if provided.
        if (queryParams.CityId.HasValue)
            query = query.Where(a => a.CityId == queryParams.CityId.Value);

        // Filter by city name containing the query string.
        if (!string.IsNullOrEmpty(queryParams.City))
            query = query.Where(a => a.City.Name.ToLower().Contains(queryParams.City.ToLower()));

        // Filter by postal code.
        if (!string.IsNullOrEmpty(queryParams.PostalCode))
            query = query.Where(a => a.PostalCode != null && a.PostalCode.Contains(queryParams.PostalCode));

        // General search across street and city name.
        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            var s = queryParams.Search.ToLower();
            query = query.Where(a =>
                a.Street.ToLower().Contains(s) ||
                a.City.Name.ToLower().Contains(s));
        }

        // Apply dynamic sorting based on street or city name.
        query = queryParams.SortField?.ToLower() switch
        {
            "street" => queryParams.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(a => a.Street)
                : query.OrderBy(a => a.Street),
            "city" => queryParams.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(a => a.City.Name)
                : query.OrderBy(a => a.City.Name),
            _ => query.OrderBy(a => a.AddressId)
        };

        var totalRecords = await query.CountAsync();

        // Paginate and project address entities into response DTOs.
        var data = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
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
            TotalPages = (int)Math.Ceiling((double)totalRecords / queryParams.PageSize),
            CurrentPage = queryParams.Page,
            PageSize = queryParams.PageSize,
            Data = data
        };
    }

    // Retrieves full address details by ID including related user and store counts.
    public async Task<AddressDetailDto?> GetAddressByIdAsync(int id)
    {
        var a = await _addressRepository.GetAddressByIdAsync(id);
        if (a == null) return null;

        // Convert the database entity into the detailed response DTO.
        return new AddressDetailDto
        {
            AddressId = a.AddressId,
            Street = a.Street,
            PostalCode = a.PostalCode,
            Phone = a.Phone,
            CityId = a.CityId,
            CityName = a.City.Name,
            CountryName = a.City.Country.Name,
            UserCount = a.Users.Count,
            StoreCount = a.Stores.Count,
            LastUpdate = a.LastUpdate,
        };
    }

    // Validates request data and creates a new address record.
    public async Task<AddressResponseDto> CreateAddressAsync(CreateAddressDto dto)
    {
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
        return new AddressResponseDto
        {
            AddressId = created.AddressId,
            Street = created.Street,
            PostalCode = created.PostalCode,
            Phone = created.Phone,
            CityId = created.CityId,
            CityName = created.City.Name,
            CountryName = created.City.Country.Name,
            LastUpdate = created.LastUpdate,
        };
    }

    // Updates an existing address record with the provided information.
    public async Task<AddressResponseDto?> UpdateAddressAsync(UpdateAddressDto dto)
    {
        var address = new Address
        {
            AddressId = dto.AddressId,
            Street = dto.Street,
            PostalCode = dto.PostalCode,
            Phone = dto.Phone,
            CityId = dto.CityId,
        };

        // Save updates via the repository.
        var updated = await _addressRepository.UpdateAddressAsync(address);
        if (updated == null) return null;

        return new AddressResponseDto
        {
            AddressId = updated.AddressId,
            Street = updated.Street,
            PostalCode = updated.PostalCode,
            Phone = updated.Phone,
            CityId = updated.CityId,
            CityName = updated.City.Name,
            CountryName = updated.City.Country.Name,
            LastUpdate = updated.LastUpdate,
        };
    }

    // Deletes an address record by ID.
    public async Task<bool> DeleteAddressAsync(int id)
        => await _addressRepository.DeleteAddressAsync(id);
}