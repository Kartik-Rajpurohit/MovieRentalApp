using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<Category> GetAllCategories()
        {
            return _context.Categories
                .Include(c => c.FilmCategories);
        }

        // Sirf category info — films alag endpoint se aayenge
        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.FilmCategories)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return await GetCategoryByIdAsync(category.CategoryId) ?? category;
        }

        public async Task<Category?> UpdateCategoryAsync(Category category)
        {
            category.LastUpdate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return await GetCategoryByIdAsync(category.CategoryId);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        // Actor pattern jaisa — paginated films fetch karne ke liye
        public IQueryable<Film> GetFilmsByCategoryId(int categoryId)
            => _context.FilmCategories
                .Where(fc => fc.CategoryId == categoryId)
                .Select(fc => fc.Film)
                .AsQueryable();
    }
}