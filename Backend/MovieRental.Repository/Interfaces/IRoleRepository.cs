using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    // Repository contract — raw DB operations only
    public interface IRoleRepository
    {
        IQueryable<Role> GetAllRoles();
        Task<Role> CreateRoleAsync(Role role);
        Task<bool> RoleExistsAsync(string roleName);
    }
}