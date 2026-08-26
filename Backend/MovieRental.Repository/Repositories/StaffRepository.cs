using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    public class StaffRepository : IStaffRepository
    {
        private readonly AppDbContext _context;
        public StaffRepository(AppDbContext context) => _context = context;

        // Returns IQueryable with User relation loaded — service applies filters on top
        public IQueryable<Staff> GetAllStaff()
        {
            return _context.Staff
                .Include(s => s.User)
                .AsQueryable();
        }

        public async Task<Staff?> GetStaffByIdAsync(int id)
        {
            // Load all relations needed for DTO mapping in service
            return await _context.Staff
                .Include(s => s.User)
                    .ThenInclude(u => u!.Role)
                .Include(s => s.User)
                    .ThenInclude(u => u!.Address)
                        .ThenInclude(a => a.City)
                            .ThenInclude(c => c.Country)
                .FirstOrDefaultAsync(s => s.StaffId == id);
        }
    }
}
