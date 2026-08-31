using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        // Returns IQueryable — service applies search, sort, pagination on top
        public IQueryable<Role> GetAllRoles()
        {
            return _context.Roles.AsQueryable();
        }

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            return await _context.Roles
                .AnyAsync(r => r.RoleName.ToLower() == roleName.ToLower());
        }

        public async Task<Role> CreateRoleAsync(Role role)
        {
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }
    }
}
