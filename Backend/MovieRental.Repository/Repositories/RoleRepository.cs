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
    }
}