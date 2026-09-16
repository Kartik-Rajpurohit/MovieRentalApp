using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    // Handles business logic for movie catalog search, filtering, creation, and relational mappings.
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;

        // Receives the movie repository needed to perform movie queries and persist changes.
        public MovieService(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        // Gets paginated movies with category, language, rating, and rental rate filters.
        public async Task<PaginatedResponseDto<MovieResponseDto>> GetAllMoviesAsync(
            PaginationInputDto pagination, MovieFilterDto filter)
        {
            // 1. Base query from repository
            var query = _movieRepository.GetAllMovies();

            // 2. Search: title, description, actor name, category name
            if (!string.IsNullOrWhiteSpace(pagination.Search))
            {
                var search = pagination.Search.ToLower();
                query = query.Where(m =>
                    m.Title.ToLower().Contains(search) ||
                    (m.Description != null && m.Description.ToLower().Contains(search)) ||
                    m.MovieCategories.Any(mc => mc.Category.Name.ToLower().Contains(search)) ||
                    m.MovieActors.Any(ma =>
                        (ma.Actor.FirstName + " " + ma.Actor.LastName).ToLower().Contains(search)));
            }

            // 3. Module Filters
            if (filter.LanguageId.HasValue)
                query = query.Where(m => m.LanguageId == filter.LanguageId.Value);

            if (filter.CategoryId.HasValue)
                query = query.Where(m => m.MovieCategories
                    .Any(mc => mc.CategoryId == filter.CategoryId.Value));

            if (!string.IsNullOrEmpty(filter.Rating))
                query = query.Where(m => m.Rating == filter.Rating);

            if (filter.ReleaseYear.HasValue)
                query = query.Where(m => m.ReleaseYear == filter.ReleaseYear.Value);

            if (filter.MinRentalRate.HasValue)
                query = query.Where(m => m.RentalRate >= filter.MinRentalRate.Value);

            if (filter.MaxRentalRate.HasValue)
                query = query.Where(m => m.RentalRate <= filter.MaxRentalRate.Value);

            if (filter.MinLength.HasValue)
                query = query.Where(m => m.Length >= filter.MinLength.Value);

            if (filter.MaxLength.HasValue)
                query = query.Where(m => m.Length <= filter.MaxLength.Value);

            // 4. Sorting: dynamic sorting on allowed fields
            var isDesc = string.Equals(pagination.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
            query = pagination.SortBy?.ToLower() switch
            {
                "title" => isDesc
                    ? query.OrderByDescending(m => m.Title)
                    : query.OrderBy(m => m.Title),
                "releaseyear" => isDesc
                    ? query.OrderByDescending(m => m.ReleaseYear)
                    : query.OrderBy(m => m.ReleaseYear),
                "rentalrate" => isDesc
                    ? query.OrderByDescending(m => m.RentalRate)
                    : query.OrderBy(m => m.RentalRate),
                "length" => isDesc
                    ? query.OrderByDescending(m => m.Length)
                    : query.OrderBy(m => m.Length),
                "id" or "movieid" => isDesc
                    ? query.OrderByDescending(m => m.MovieId)
                    : query.OrderBy(m => m.MovieId),
                _ => isDesc
                    ? query.OrderByDescending(m => m.Title)
                    : query.OrderBy(m => m.Title)
            };

            // 5. Total count before pagination
            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pagination.PageSize);

            // 6. Pagination & Materialization
            var entities = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            var data = entities.Select(m => MapToResponseDto(m)).ToList();

            return new PaginatedResponseDto<MovieResponseDto>
            {
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = pagination.Page,
                PageSize = pagination.PageSize,
                Data = data
            };
        }

        // Gets movie detail by ID including categories, actors, and inventory copies.
        public async Task<MovieDetailDto?> GetMovieByIdAsync(int id)
        {
            var movie = await _movieRepository.GetMovieByIdAsync(id);
            if (movie == null) return null;
            return MapToDetailDto(movie);
        }

        // Creates a new movie and links associated category and actor relationships.
        public async Task<MovieResponseDto> CreateMovieAsync(CreateMovieDto dto)
        {
            // Validate language references
            var languageExists = await _movieRepository.GetAllLanguages().AnyAsync(l => l.LanguageId == dto.LanguageId);
            if (!languageExists)
            {
                throw new InvalidOperationException($"Language with ID {dto.LanguageId} does not exist or has been deleted.");
            }

            if (dto.OriginalLanguageId.HasValue)
            {
                var origLanguageExists = await _movieRepository.GetAllLanguages().AnyAsync(l => l.LanguageId == dto.OriginalLanguageId.Value);
                if (!origLanguageExists)
                {
                    throw new InvalidOperationException($"Original Language with ID {dto.OriginalLanguageId.Value} does not exist or has been deleted.");
                }
            }

            // Validate category references
            if (dto.CategoryIds != null && dto.CategoryIds.Any())
            {
                var activeCategoryIds = await _movieRepository.GetAllCategories()
                    .Where(c => dto.CategoryIds.Contains(c.CategoryId))
                    .Select(c => c.CategoryId)
                    .ToListAsync();
                var missingCategories = dto.CategoryIds.Except(activeCategoryIds).ToList();
                if (missingCategories.Any())
                {
                    throw new InvalidOperationException($"One or more categories do not exist or have been deleted: {string.Join(", ", missingCategories)}");
                }
            }

            // Validate actor references
            if (dto.ActorIds != null && dto.ActorIds.Any())
            {
                var activeActorIds = await _movieRepository.GetAllActors()
                    .Where(a => dto.ActorIds.Contains(a.ActorId))
                    .Select(a => a.ActorId)
                    .ToListAsync();
                var missingActors = dto.ActorIds.Except(activeActorIds).ToList();
                if (missingActors.Any())
                {
                    throw new InvalidOperationException($"One or more actors do not exist or have been deleted: {string.Join(", ", missingActors)}");
                }
            }

            // Build Movie entity from DTO
            var movie = new Movie
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
                MovieCategories = (dto.CategoryIds ?? []).Select(cid => new MovieCategory
                {
                    CategoryId = cid,
                    LastUpdate = DateTime.UtcNow
                }).ToList(),

                // Link actors via junction table
                MovieActors = (dto.ActorIds ?? []).Select(aid => new MovieActor
                {
                    ActorId = aid,
                    LastUpdate = DateTime.UtcNow
                }).ToList()
            };

            var created = await _movieRepository.CreateMovieAsync(movie);
            return MapToResponseDto(created);
        }

        // Updates an existing movie and refreshes category/actor links.
        public async Task<MovieResponseDto?> UpdateMovieAsync(UpdateMovieDto dto)
        {
            // Fetch existing entity
            var movie = await _movieRepository.GetMovieByIdAsync(dto.MovieId);
            if (movie == null) return null;

            // Validate language references if provided
            if (dto.LanguageId.HasValue)
            {
                var languageExists = await _movieRepository.GetAllLanguages().AnyAsync(l => l.LanguageId == dto.LanguageId.Value);
                if (!languageExists)
                {
                    throw new InvalidOperationException($"Language with ID {dto.LanguageId.Value} does not exist or has been deleted.");
                }
            }

            if (dto.OriginalLanguageId.HasValue)
            {
                var origLanguageExists = await _movieRepository.GetAllLanguages().AnyAsync(l => l.LanguageId == dto.OriginalLanguageId.Value);
                if (!origLanguageExists)
                {
                    throw new InvalidOperationException($"Original Language with ID {dto.OriginalLanguageId.Value} does not exist or has been deleted.");
                }
            }

            // Validate category references if provided
            if (dto.CategoryIds != null && dto.CategoryIds.Any())
            {
                var activeCategoryIds = await _movieRepository.GetAllCategories()
                    .Where(c => dto.CategoryIds.Contains(c.CategoryId))
                    .Select(c => c.CategoryId)
                    .ToListAsync();
                var missingCategories = dto.CategoryIds.Except(activeCategoryIds).ToList();
                if (missingCategories.Any())
                {
                    throw new InvalidOperationException($"One or more categories do not exist or have been deleted: {string.Join(", ", missingCategories)}");
                }
            }

            // Validate actor references if provided
            if (dto.ActorIds != null && dto.ActorIds.Any())
            {
                var activeActorIds = await _movieRepository.GetAllActors()
                    .Where(a => dto.ActorIds.Contains(a.ActorId))
                    .Select(a => a.ActorId)
                    .ToListAsync();
                var missingActors = dto.ActorIds.Except(activeActorIds).ToList();
                if (missingActors.Any())
                {
                    throw new InvalidOperationException($"One or more actors do not exist or have been deleted: {string.Join(", ", missingActors)}");
                }
            }

            // Only update fields that were actually sent — PATCH behaviour
            if (!string.IsNullOrWhiteSpace(dto.Title)) movie.Title = dto.Title;
            if (dto.Description != null) movie.Description = dto.Description;
            if (dto.ReleaseYear.HasValue) movie.ReleaseYear = dto.ReleaseYear;
            if (dto.LanguageId.HasValue) movie.LanguageId = dto.LanguageId.Value;
            if (dto.OriginalLanguageId.HasValue) movie.OriginalLanguageId = dto.OriginalLanguageId;
            if (dto.RentalDuration.HasValue) movie.RentalDuration = dto.RentalDuration.Value;
            if (dto.RentalRate.HasValue) movie.RentalRate = dto.RentalRate.Value;
            if (dto.Length.HasValue) movie.Length = dto.Length;
            if (dto.ReplacementCost.HasValue) movie.ReplacementCost = dto.ReplacementCost.Value;
            if (dto.Rating != null) movie.Rating = dto.Rating;
            if (dto.SpecialFeatures != null) movie.SpecialFeatures = dto.SpecialFeatures;

            // Replace categories if provided
            if (dto.CategoryIds != null)
            {
                movie.MovieCategories = dto.CategoryIds.Select(cid => new MovieCategory
                {
                    MovieId = movie.MovieId,
                    CategoryId = cid,
                    LastUpdate = DateTime.UtcNow
                }).ToList();
            }

            // Replace actors if provided
            if (dto.ActorIds != null)
            {
                movie.MovieActors = dto.ActorIds.Select(aid => new MovieActor
                {
                    MovieId = movie.MovieId,
                    ActorId = aid,
                    LastUpdate = DateTime.UtcNow
                }).ToList();
            }

            var updated = await _movieRepository.UpdateMovieAsync(movie);
            return updated == null ? null : MapToResponseDto(updated);
        }

        // Deletes a movie record.
        public async Task<bool> DeleteMovieAsync(int id)
        {
            return await _movieRepository.DeleteMovieAsync(id);
        }

        // Retrieves a paginated list of languages formatted for dropdown selectors.
        public async Task<IEnumerable<DropdownDto>> GetAllLanguagesAsync(int page, int pageSize)
        {
            return await _movieRepository.GetAllLanguages()
                .OrderBy(l => l.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new DropdownDto { Id = l.LanguageId, Name = l.Name })
                .ToListAsync();
        }

        // Retrieves a paginated list of categories formatted for dropdown selectors.
        public async Task<IEnumerable<DropdownDto>> GetAllCategoriesAsync(int page, int pageSize)
        {
            return await _movieRepository.GetAllCategories()
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new DropdownDto { Id = c.CategoryId, Name = c.Name })
                .ToListAsync();
        }

        // Retrieves a paginated list of actors formatted for dropdown selectors.
        public async Task<IEnumerable<DropdownDto>> GetAllActorsAsync(int page, int pageSize)
        {
            return await _movieRepository.GetAllActors()
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

        // Private helpers — maps Movie entity to DTOs

        private static MovieResponseDto MapToResponseDto(Movie m) => new MovieResponseDto
        {
            MovieId = m.MovieId,
            Title = m.Title,
            Description = m.Description,
            ReleaseYear = m.ReleaseYear,
            LanguageId = m.LanguageId,
            LanguageName = m.Language?.Name ?? "",
            RentalDuration = m.RentalDuration,
            RentalRate = m.RentalRate,
            Length = m.Length,
            ReplacementCost = m.ReplacementCost,
            Rating = m.Rating,
            Categories = m.MovieCategories
                .Select(mc => mc.Category?.Name ?? "")
                .Where(n => n != "")
                .ToList(),
            Actors = m.MovieActors
                .Select(ma => $"{ma.Actor?.FirstName} {ma.Actor?.LastName}".Trim())
                .Where(n => n != "")
                .ToList()
        };

        private static MovieDetailDto MapToDetailDto(Movie m) => new MovieDetailDto
        {
            MovieId = m.MovieId,
            Title = m.Title,
            Description = m.Description,
            ReleaseYear = m.ReleaseYear,
            LanguageId = m.LanguageId,
            LanguageName = m.Language?.Name ?? "",
            OriginalLanguageId = m.OriginalLanguageId,
            OriginalLanguageName = m.OriginalLanguage?.Name,
            RentalDuration = m.RentalDuration,
            RentalRate = m.RentalRate,
            Length = m.Length,
            ReplacementCost = m.ReplacementCost,
            Rating = m.Rating,
            SpecialFeatures = m.SpecialFeatures,
            Categories = m.MovieCategories.Select(mc => new CategoryDto
            {
                CategoryId = mc.CategoryId,
                Name = mc.Category?.Name ?? ""
            }).ToList(),
            Actors = m.MovieActors.Select(ma => new ActorDto
            {
                ActorId = ma.ActorId,
                FullName = $"{ma.Actor?.FirstName} {ma.Actor?.LastName}".Trim()
            }).ToList(),
            TotalInventory = m.Inventories?.Count ?? 0
        };
    }
}
