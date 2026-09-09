using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    // Defines database operations for Category entities and genre film listings.
    public interface ICategoryRepository
    {
        // Returns a queryable collection of all categories with their film links.
        IQueryable<Category> GetAllCategories();

        // Finds a category by its ID.
        Task<Category?> GetCategoryByIdAsync(int id);

        // Adds a new category to the database.
        Task<Category> CreateCategoryAsync(Category category);

        // Updates an existing category's name.
        Task<Category?> UpdateCategoryAsync(Category category);

        // Deletes a category by ID; returns true if deleted, false if not found.
        Task<bool> DeleteCategoryAsync(int id);

        // Returns a queryable collection of films that belong to the specified category.
        IQueryable<Film> GetFilmsByCategoryId(int categoryId);
    }
}