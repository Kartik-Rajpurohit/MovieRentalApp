using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Locations.Addresses;
using MovieRental.Domain.QueryParameters;

namespace MovieRental.Services.Interfaces;

// Defines business operations for managing physical addresses.
public interface IAddressService
{
    // Retrieves a paginated and filtered list of addresses.
    Task<PaginatedResponseDto<AddressResponseDto>> GetAllAddressesAsync(AddressQueryParametersDto queryParams);

    // Retrieves full address details by ID, including city and country.
    Task<AddressDetailDto?> GetAddressByIdAsync(int id);

    // Creates a new address record in the database.
    Task<AddressResponseDto> CreateAddressAsync(CreateAddressDto dto);

    // Updates an existing address record.
    Task<AddressResponseDto?> UpdateAddressAsync(UpdateAddressDto dto);

    // Deletes an address by ID.
    Task<bool> DeleteAddressAsync(int id);
}