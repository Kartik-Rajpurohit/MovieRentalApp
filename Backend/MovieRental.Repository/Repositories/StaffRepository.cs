using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    // Handles database operations related to staff members.
    public class StaffRepository : IStaffRepository
    {
        // Receives the database context used to access staff data.
        private readonly AppDbContext _context;
        public StaffRepository(AppDbContext context) => _context = context;

        // Reads all staff members without tracking, loading their linked User account.
        public IQueryable<Staff> GetAllStaff()
        {
            return _context.Staff
                .AsNoTracking()
                .Include(s => s.User)
                .AsQueryable();
        }

        // Finds a staff member by ID, loading full User, Role, Address, City, and Country trees.
        public async Task<Staff?> GetStaffByIdAsync(int id)
        {
            return await _context.Staff
                .Include(s => s.User)
                    .ThenInclude(u => u!.Role)
                .Include(s => s.User)
                    .ThenInclude(u => u!.Address)
                        .ThenInclude(a => a!.City)
                            .ThenInclude(c => c!.Country)
                .FirstOrDefaultAsync(s => s.StaffId == id);
        }
    }
}
