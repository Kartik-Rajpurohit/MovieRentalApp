using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    // Defines database operations for Staff entities linked to User and Store records.
    public interface IStaffRepository
    {
        // Returns a queryable collection of staff members with linked user accounts.
        IQueryable<Staff> GetAllStaff();

        // Finds a staff member by their ID with full user, store, and address info.
        Task<Staff?> GetStaffByIdAsync(int id);
    }
}