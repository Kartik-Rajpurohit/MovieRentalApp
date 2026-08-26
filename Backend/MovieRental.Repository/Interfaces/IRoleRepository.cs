using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    // Repository contract — raw DB operations only
    public interface IRoleRepository
    {
        IQueryable<Role> GetAllRoles();
    }
}