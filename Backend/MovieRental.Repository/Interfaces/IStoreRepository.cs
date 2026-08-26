using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    public interface IStoreRepository
    {
        IQueryable<Store> GetAllStores();
    }
}
