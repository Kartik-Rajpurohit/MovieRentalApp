using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.Entities;
using MovieRental.Domain.QueryParameters;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    public class FilmService : IFilmService
    {
        private readonly IFilmRepository _filmRepository;

        public FilmService(IFilmRepository filmRepository)
        {
            _filmRepository = filmRepository;
        }

        public async Task<PaginatedResponseDto<MovieResponseDto>> GetAllFilmsAsync(MovieQueryParametersDto queryParams)
        {
            var query = _filmRepository.GetAllFilms();

            // Filter by language
            if (queryParams.LanguageId.HasValue)
                query = query.Where(f => f.LanguageId == queryParams.LanguageId.Value);

            // Filter by category
            if (queryParams.CategoryId.HasValue)
                query = query.Where(f => f.FilmCategories
                    .Any(fc => fc.CategoryId == queryParams.CategoryId.Value));

            // Filter by MPAA rating
            if (!string.IsNullOrEmpty(queryParams.Rating))
                query = query.Where(f => f.Rating == queryParams.Rating);

            // Filter by release year
            if (queryParams.ReleaseYear.HasValue)
                query = query.Where(f => f.ReleaseYear == queryParams.ReleaseYear.Value);

            // Global search — title, description, actor name, category name
            if (!string.IsNullOrEmpty(queryParams.Search))
            {
                var search = queryParams.Search.ToLower();
                query = query.Where(f =>
                    f.Title.ToLower().Contains(search) ||
                    (f.Description != null && f.Description.ToLower().Contains(search)) ||
                    f.FilmCategories.Any(fc => fc.Category.Name.ToLower().Contains(search)) ||
                    f.FilmActors.Any(fa =>
                        (fa.Actor.FirstName + " " + fa.Actor.LastName).ToLower().Contains(search)));
            }

            // Sorting
            if (!string.IsNullOrEmpty(queryParams.SortField))
            {
                query = queryParams.SortField.ToLower() switch
                {
                    "title" => queryParams.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(f => f.Title)
                        : query.OrderBy(f => f.Title),

                    "releaseyear" => queryParams.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(f => f.ReleaseYear)
                        : query.OrderBy(f => f.ReleaseYear),

                    "rentalrate" => queryParams.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(f => f.RentalRate)
                        : query.OrderBy(f => f.RentalRate),

                    "length" => queryParams.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(f => f.Length)
                        : query.OrderBy(f => f.Length),

                    _ => query.OrderBy(f => f.Title)
                };
            }
            else
            {
                // Default sort by title
                query = query.OrderBy(f => f.Title);
            }

            // Total count before pagination
            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / queryParams.PageSize);

            // Fetch current page — map in memory
            var entities = await query
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            var data = entities.Select(f => MapToResponseDto(f)).ToList();

            return new PaginatedResponseDto<MovieResponseDto>
            {
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = queryParams.Page,
                PageSize = queryParams.PageSize,
                Data = data
            };
        }

        public async Task<MovieDetailDto?> GetFilmByIdAsync(int id)
        {
            var film = await _filmRepository.GetFilmByIdAsync(id);
            if (film == null) return null;
            return MapToDetailDto(film);
        }

        public async Task<MovieResponseDto> CreateFilmAsync(CreateMovieDto dto)
        {
            // Build Film entity from DTO
            var film = new Film
            {
                Title = dto.Title,
                Description = dto.Description,
                ReleaseYear = dto.ReleaseYear,
                LanguageId = dto.LanguageId,
                OriginalLanguageId = dto.OriginalLanguageId,
                RentalDuration = dto.RentalDuration,
                RentalRate = dto.RentalRate,
                Length = dto.Length,
                ReplacementCost = dto.ReplacementCost,
                Rating = dto.Rating,
                SpecialFeatures = dto.SpecialFeatures,
                LastUpdate = DateTime.UtcNow,

                // Link categories via junction table
                FilmCategories = dto.CategoryIds.Select(cid => new FilmCategory
                {
                    CategoryId = cid,
                    LastUpdate = DateTime.UtcNow
                }).ToList(),

                // Link actors via junction table
                FilmActors = dto.ActorIds.Select(aid => new FilmActor
                {
                    ActorId = aid,
                    LastUpdate = DateTime.UtcNow
                }).ToList()
            };

            var created = await _filmRepository.CreateFilmAsync(film);
            return MapToResponseDto(created);
        }

        public async Task<MovieResponseDto?> UpdateFilmAsync(UpdateMovieDto dto)
        {
            // Fetch existing entity
            var film = await _filmRepository.GetFilmByIdAsync(dto.FilmId);
            if (film == null) return null;

            // Only update fields that were actually sent — PATCH behaviour
            if (!string.IsNullOrWhiteSpace(dto.Title)) film.Title = dto.Title;
            if (dto.Description != null) film.Description = dto.Description;
            if (dto.ReleaseYear.HasValue) film.ReleaseYear = dto.ReleaseYear;
            if (dto.LanguageId.HasValue) film.LanguageId = dto.LanguageId.Value;
            if (dto.OriginalLanguageId.HasValue) film.OriginalLanguageId = dto.OriginalLanguageId;
            if (dto.RentalDuration.HasValue) film.RentalDuration = dto.RentalDuration.Value;
            if (dto.RentalRate.HasValue) film.RentalRate = dto.RentalRate.Value;
            if (dto.Length.HasValue) film.Length = dto.Length;
            if (dto.ReplacementCost.HasValue) film.ReplacementCost = dto.ReplacementCost.Value;
            if (dto.Rating != null) film.Rating = dto.Rating;
            if (dto.SpecialFeatures != null) film.SpecialFeatures = dto.SpecialFeatures;

            // Replace categories if provided
            if (dto.CategoryIds != null)
            {
                film.FilmCategories = dto.CategoryIds.Select(cid => new FilmCategory
                {
                    FilmId = film.FilmId,
                    CategoryId = cid,
                    LastUpdate = DateTime.UtcNow
                }).ToList();
            }

            // Replace actors if provided
            if (dto.ActorIds != null)
            {
                film.FilmActors = dto.ActorIds.Select(aid => new FilmActor
                {
                    FilmId = film.FilmId,
                    ActorId = aid,
                    LastUpdate = DateTime.UtcNow
                }).ToList();
            }

            var updated = await _filmRepository.UpdateFilmAsync(film);
            return updated == null ? null : MapToResponseDto(updated);
        }

        public async Task<bool> DeleteFilmAsync(int id)
        {
            return await _filmRepository.DeleteFilmAsync(id);
        }

        public async Task<IEnumerable<DropdownDto>> GetAllLanguagesAsync(int page, int pageSize)
        {
            return await _filmRepository.GetAllLanguages()
                .OrderBy(l => l.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new DropdownDto { Id = l.LanguageId, Name = l.Name })
                .ToListAsync();
        }

        public async Task<IEnumerable<DropdownDto>> GetAllCategoriesAsync(int page, int pageSize)
        {
            return await _filmRepository.GetAllCategories()
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new DropdownDto { Id = c.CategoryId, Name = c.Name })
                .ToListAsync();
        }

        public async Task<IEnumerable<DropdownDto>> GetAllActorsAsync(int page, int pageSize)
        {
            return await _filmRepository.GetAllActors()
                .OrderBy(a => a.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new DropdownDto
                {
                    Id = a.ActorId,
                    Name = a.FirstName + " " + a.LastName
                })
                .ToListAsync();
        }

        // Private helpers — maps Film entity to DTOs

        private static MovieResponseDto MapToResponseDto(Film f) => new MovieResponseDto
        {
            FilmId = f.FilmId,
            Title = f.Title,
            Description = f.Description,
            ReleaseYear = f.ReleaseYear,
            LanguageId = f.LanguageId,
            LanguageName = f.Language?.Name ?? "",
            RentalDuration = f.RentalDuration,
            RentalRate = f.RentalRate,
            Length = f.Length,
            ReplacementCost = f.ReplacementCost,
            Rating = f.Rating,
            Categories = f.FilmCategories
                .Select(fc => fc.Category?.Name ?? "")
                .Where(n => n != "")
                .ToList(),
            Actors = f.FilmActors
                .Select(fa => $"{fa.Actor?.FirstName} {fa.Actor?.LastName}".Trim())
                .Where(n => n != "")
                .ToList()
        };

        private static MovieDetailDto MapToDetailDto(Film f) => new MovieDetailDto
        {
            FilmId = f.FilmId,
            Title = f.Title,
            Description = f.Description,
            ReleaseYear = f.ReleaseYear,
            LanguageId = f.LanguageId,
            LanguageName = f.Language?.Name ?? "",
            OriginalLanguageId = f.OriginalLanguageId,
            OriginalLanguageName = f.OriginalLanguage?.Name,
            RentalDuration = f.RentalDuration,
            RentalRate = f.RentalRate,
            Length = f.Length,
            ReplacementCost = f.ReplacementCost,
            Rating = f.Rating,
            SpecialFeatures = f.SpecialFeatures,
            Categories = f.FilmCategories.Select(fc => new CategoryDto
            {
                CategoryId = fc.CategoryId,
                Name = fc.Category?.Name ?? ""
            }).ToList(),
            Actors = f.FilmActors.Select(fa => new ActorDto
            {
                ActorId = fa.ActorId,
                FullName = $"{fa.Actor?.FirstName} {fa.Actor?.LastName}".Trim()
            }).ToList(),
            TotalInventory = f.Inventories?.Count ?? 0
        };
    }
}
