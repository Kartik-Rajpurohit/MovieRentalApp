using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Locations.Addresses;
using MovieRental.Domain.QueryParameters;

namespace MovieRental.Services.Interfaces;

public interface IAddressService
{
    Task<PaginatedResponseDto<AddressResponseDto>> GetAllAddressesAsync(AddressQueryParametersDto queryParams);
    Task<AddressDetailDto?> GetAddressByIdAsync(int id);
    Task<AddressResponseDto> CreateAddressAsync(CreateAddressDto dto);
    Task<AddressResponseDto?> UpdateAddressAsync(UpdateAddressDto dto);
    Task<bool> DeleteAddressAsync(int id);
}