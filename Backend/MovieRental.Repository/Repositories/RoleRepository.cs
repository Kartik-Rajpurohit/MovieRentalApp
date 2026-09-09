using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    // Handles database operations related to user roles.
    public class RoleRepository : IRoleRepository
    {
        // Receives the database context used to access role tables.
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        // Reads all roles from the database without tracking.
        public IQueryable<Role> GetAllRoles()
        {
            return _context.Roles.AsNoTracking().AsQueryable();
        }

        // Checks whether a role with the same name already exists (case-insensitive).
        public async Task<bool> RoleExistsAsync(string roleName)
        {
            return await _context.Roles
                .AnyAsync(r => r.RoleName.ToLower() == roleName.ToLower());
        }

        // Adds a new role to the database and saves changes.
        public async Task<Role> CreateRoleAsync(Role role)
        {
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }
    }
}
