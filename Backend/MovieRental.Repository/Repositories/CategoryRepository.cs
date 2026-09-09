using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    // Handles database operations related to film categories and genres.
    public class CategoryRepository : ICategoryRepository
    {
        // Receives the database context used to access category data.
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        // Reads all categories without tracking, including film links for counting.
        public IQueryable<Category> GetAllCategories()
        {
            return _context.Categories
                .AsNoTracking()
                .Include(c => c.FilmCategories);
        }

        // Finds a category by its ID.
        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.FilmCategories)
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

        // Removes the category from the database if found.
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        // Queries films belonging to this category through the FilmCategory join table.
        public IQueryable<Film> GetFilmsByCategoryId(int categoryId)
            => _context.FilmCategories
                .AsNoTracking()
                .Where(fc => fc.CategoryId == categoryId)
                .Select(fc => fc.Film)
                .AsQueryable();
    }
}