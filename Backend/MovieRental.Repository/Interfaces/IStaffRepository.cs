using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    // Repository contract — raw DB operations only
    public interface IStaffRepository
    {
        IQueryable<Staff> GetAllStaff();
        Task<Staff?> GetStaffByIdAsync(int id); // Returns raw entity — service handles mapping
    }
}