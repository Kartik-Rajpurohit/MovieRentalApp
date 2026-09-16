using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    // Handles database operations related to movie categories and genres.
    public class CategoryRepository : ICategoryRepository
    {
        // Receives the database context used to access category data.
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        // Reads all active categories without tracking, including movie links for counting.
        public IQueryable<Category> GetAllCategories()
        {
            return _context.Categories
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .Include(c => c.MovieCategories);
        }

        // Finds an active category by its ID.
        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted)
                .Include(c => c.MovieCategories)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        // Adds a new category to the database and returns the created record.
        public async Task<Category> CreateCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return await GetCategoryByIdAsync(category.CategoryId) ?? category;
        }

        // Updates the category timestamp and saves changes.
        public async Task<Category?> UpdateCategoryAsync(Category category)
        {
            category.LastUpdate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return await GetCategoryByIdAsync(category.CategoryId);
        }

        // Soft-deletes the category from the database if found.
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null || category.IsDeleted) return false;
            category.IsDeleted = true;
            category.LastUpdate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // Queries active movies belonging to this category through the MovieCategory join table.
        public IQueryable<Movie> GetMoviesByCategoryId(int categoryId)
            => _context.MovieCategories
                .AsNoTracking()
                .Where(mc => mc.CategoryId == categoryId && !mc.Movie.IsDeleted)
                .Select(mc => mc.Movie)
                .AsQueryable();
    }
}