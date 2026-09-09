using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    // Defines database operations for system Role entities (Admin, Staff, Customer).
    public interface IRoleRepository
    {
        // Returns a queryable collection of all roles.
        IQueryable<Role> GetAllRoles();

        // Adds a new role to the database.
        Task<Role> CreateRoleAsync(Role role);

        // Checks whether a role with the given name already exists.
        Task<bool> RoleExistsAsync(string roleName);
    }
}