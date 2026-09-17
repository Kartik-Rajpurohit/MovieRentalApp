using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;

namespace MovieRental.Services.Extensions;

// Provides reusable, database-side pagination and projection for dropdown lookup queries.
public static class DropdownExtensions
{
    // Applies ordering, pagination, and projection to produce dropdown items database-side.
    public static async Task<IEnumerable<DropdownDto>> ToDropdownListAsync<TEntity, TKey>(
        this IQueryable<TEntity> query,
        Expression<Func<TEntity, TKey>> orderBySelector,
        Expression<Func<TEntity, DropdownDto>> projection,
        int page,
        int pageSize)
    {
        return await query
            .OrderBy(orderBySelector)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(projection)
            .ToListAsync();
    }
}
