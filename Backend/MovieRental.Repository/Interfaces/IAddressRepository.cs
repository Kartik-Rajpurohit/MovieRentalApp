using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces;

public interface IAddressRepository
{
    IQueryable<Address> GetAllAddresses();
    Task<Address?> GetAddressByIdAsync(int id);
    Task<Address> CreateAddressAsync(Address address);
    Task<Address?> UpdateAddressAsync(Address address);
    Task<bool> DeleteAddressAsync(int id);
}