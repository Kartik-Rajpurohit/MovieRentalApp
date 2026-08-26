using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    public interface ICategoryRepository
    {
        IQueryable<Category> GetAllCategories();
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category?> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(int id);
        IQueryable<Film> GetFilmsByCategoryId(int categoryId);
    }
}