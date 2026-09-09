namespace MovieRental.Domain.DTOs.Stores;

// Request DTO sent by the client to create a new physical store location
public class CreateStoreDto
{
    // Staff ID of the manager assigned to this store
    public int? ManagerStaffId { get; set; }

    // Address ID where the store is located
    public int AddressId { get; set; }
}