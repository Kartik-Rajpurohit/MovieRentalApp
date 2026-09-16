using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    // Defines database operations for Category entities and genre movie listings.
    public interface ICategoryRepository
    {
        // Returns a queryable collection of all categories with their movie links.
        IQueryable<Category> GetAllCategories();

        // Finds a category by its ID.
        Task<Category?> GetCategoryByIdAsync(int id);

        // Adds a new category to the database.
        Task<Category> CreateCategoryAsync(Category category);

        // Updates an existing category's name.
        Task<Category?> UpdateCategoryAsync(Category category);

        // Deletes a category by ID; returns true if deleted, false if not found.
        Task<bool> DeleteCategoryAsync(int id);

        // Returns a queryable collection of movies that belong to the specified category.
        IQueryable<Movie> GetMoviesByCategoryId(int categoryId);
    }
}