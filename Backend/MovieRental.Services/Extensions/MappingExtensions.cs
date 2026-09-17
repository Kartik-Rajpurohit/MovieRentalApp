using System.Linq.Expressions;
using MovieRental.Domain.DTOs.Actors;
using MovieRental.Domain.DTOs.Categories;
using MovieRental.Domain.DTOs.Customers;
using MovieRental.Domain.DTOs.Inventory;
using MovieRental.Domain.DTOs.Languages;
using MovieRental.Domain.DTOs.Locations.Addresses;
using MovieRental.Domain.DTOs.Locations.Cities;
using MovieRental.Domain.DTOs.Locations.Countries;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.DTOs.Payments;
using MovieRental.Domain.DTOs.Rentals;
using MovieRental.Domain.DTOs.Roles;
using MovieRental.Domain.DTOs.Staff;
using MovieRental.Domain.DTOs.Stores;
using MovieRental.Domain.DTOs.Users;
using MovieRental.Domain.Entities;

namespace MovieRental.Services.Extensions;

// Centralizes reusable Entity -> DTO mapping extension methods and EF Core translatable expressions.
public static class MappingExtensions
{
    // ==========================================
    // EF CORE IQUERYABLE PROJECTIONS
    // ==========================================

    // Database-translatable projection for movie catalog listing and subcollections.
    public static readonly Expression<Func<Movie, MovieResponseDto>> MovieSummaryProjection =
        m => new MovieResponseDto
        {
            MovieId = m.MovieId,
            Title = m.Title,
            Description = m.Description,
            ReleaseYear = m.ReleaseYear,
            LanguageId = m.LanguageId,
            LanguageName = m.Language != null ? m.Language.Name : "",
            RentalDuration = m.RentalDuration,
            RentalRate = m.RentalRate,
            Length = m.Length,
            ReplacementCost = m.ReplacementCost,
            Rating = m.Rating,
            Categories = m.MovieCategories
                .Select(mc => mc.Category.Name)
                .ToList()
        };

    // Projects an IQueryable of Movie entities to MovieResponseDto database-side.
    public static IQueryable<MovieResponseDto> ProjectToMovieResponseDto(this IQueryable<Movie> query)
        => query.Select(MovieSummaryProjection);


    // ==========================================
    // IN-MEMORY MATERIALIZED ENTITY EXTENSIONS
    // ==========================================

    #region Movie Mappings

    public static MovieResponseDto ToResponseDto(this Movie m) => new()
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
        Categories = m.MovieCategories?
            .Select(mc => mc.Category?.Name ?? "")
            .Where(n => n != "")
            .ToList() ?? new List<string>(),
        Actors = m.MovieActors?
            .Select(ma => $"{ma.Actor?.FirstName} {ma.Actor?.LastName}".Trim())
            .Where(n => n != "")
            .ToList() ?? new List<string>()
    };

    public static MovieDetailDto ToDetailDto(this Movie m) => new()
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
        Categories = m.MovieCategories?.Select(mc => new CategoryDto
        {
            CategoryId = mc.CategoryId,
            Name = mc.Category?.Name ?? ""
        }).ToList() ?? new List<CategoryDto>(),
        Actors = m.MovieActors?.Select(ma => new ActorDto
        {
            ActorId = ma.ActorId,
            FullName = $"{ma.Actor?.FirstName} {ma.Actor?.LastName}".Trim()
        }).ToList() ?? new List<ActorDto>(),
        TotalInventory = m.Inventories?.Count ?? 0
    };

    #endregion

    #region Actor Mappings

    public static ActorResponseDto ToResponseDto(this Actor a) => new()
    {
        ActorId = a.ActorId,
        FirstName = a.FirstName,
        LastName = a.LastName,
        LastUpdate = a.LastUpdate,
        MovieCount = a.MovieActors?.Count ?? 0
    };

    #endregion

    #region Category Mappings

    public static CategoryResponseDto ToResponseDto(this Category c) => new()
    {
        CategoryId = c.CategoryId,
        Name = c.Name,
        LastUpdate = c.LastUpdate,
        MovieCount = c.MovieCategories?.Count ?? 0
    };

    #endregion

    #region Language Mappings

    public static LanguageResponseDto ToResponseDto(this Language l) => new()
    {
        LanguageId = l.LanguageId,
        Name = l.Name,
        LastUpdate = l.LastUpdate,
        MovieCount = l.Movies?.Count ?? 0
    };

    #endregion

    #region Address Mappings

    public static AddressResponseDto ToResponseDto(this Address a) => new()
    {
        AddressId = a.AddressId,
        Street = a.Street,
        PostalCode = a.PostalCode,
        Phone = a.Phone,
        CityId = a.CityId,
        CityName = a.City?.Name ?? "",
        CountryName = a.City?.Country?.Name ?? "",
        LastUpdate = a.LastUpdate
    };

    public static AddressDetailDto ToDetailDto(this Address a) => new()
    {
        AddressId = a.AddressId,
        Street = a.Street,
        PostalCode = a.PostalCode,
        Phone = a.Phone,
        CityId = a.CityId,
        CityName = a.City?.Name ?? "",
        CountryName = a.City?.Country?.Name ?? "",
        UserCount = a.Users?.Count ?? 0,
        StoreCount = a.Stores?.Count ?? 0,
        LastUpdate = a.LastUpdate
    };

    #endregion

    #region City Mappings

    public static CityResponseDto ToResponseDto(this City c) => new()
    {
        CityId = c.CityId,
        Name = c.Name,
        CountryId = c.CountryId,
        CountryName = c.Country?.Name ?? "",
        AddressCount = c.Addresses?.Count ?? 0,
        LastUpdate = c.LastUpdate
    };

    public static CityDetailDto ToDetailDto(this City c) => new()
    {
        CityId = c.CityId,
        Name = c.Name,
        CountryId = c.CountryId,
        CountryName = c.Country?.Name ?? "",
        AddressCount = c.Addresses?.Count ?? 0,
        LastUpdate = c.LastUpdate
    };

    #endregion

    #region Country Mappings

    public static CountryResponseDto ToResponseDto(this Country c) => new()
    {
        CountryId = c.CountryId,
        Name = c.Name,
        CityCount = c.Cities?.Count ?? 0,
        LastUpdate = c.LastUpdate
    };

    #endregion

    #region User Mappings

    public static UserResponseDto ToResponseDto(this User u) => new()
    {
        UserId = u.UserId,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Email = u.Email,
        IsActive = u.IsActive,
        RoleId = u.RoleId,
        RoleName = u.Role?.RoleName ?? "Unassigned",
        AddressId = u.AddressId,
        Street = u.Address?.Street,
        PostalCode = u.Address?.PostalCode,
        Phone = u.Address?.Phone,
        CityName = u.Address?.City?.Name,
        CountryName = u.Address?.City?.Country?.Name
    };

    #endregion

    #region Staff Mappings

    public static StaffResponseDto ToResponseDto(this Staff s) => new()
    {
        StaffId = s.StaffId,
        FullName = s.User != null ? $"{s.User.FirstName} {s.User.LastName}".Trim() : "—",
        Email = s.User?.Email,
        StoreId = s.StoreId,
        IsActive = s.User != null && s.User.IsActive
    };

    public static StaffDetailDto ToDetailDto(this Staff s) => new()
    {
        StaffId = s.StaffId,
        FullName = s.User != null ? $"{s.User.FirstName} {s.User.LastName}".Trim() : "—",
        Email = s.User?.Email,
        IsActive = s.User?.IsActive ?? false,
        StoreId = s.StoreId,
        Street = s.User?.Address?.Street,
        PostalCode = s.User?.Address?.PostalCode,
        Phone = s.User?.Address?.Phone,
        CityName = s.User?.Address?.City?.Name,
        CountryName = s.User?.Address?.City?.Country?.Name
    };

    #endregion

    #region Customer Mappings

    public static CustomerResponseDto ToResponseDto(this Customer c) => new()
    {
        CustomerId = c.CustomerId,
        FullName = c.User != null ? $"{c.User.FirstName} {c.User.LastName}".Trim() : "—",
        Email = c.User?.Email,
        StoreId = c.StoreId,
        IsActive = c.User != null && c.User.IsActive,
        CreateDate = c.CreateDate
    };

    public static CustomerDetailDto ToDetailDto(this Customer c) => new()
    {
        CustomerId = c.CustomerId,
        FullName = c.User != null ? $"{c.User.FirstName} {c.User.LastName}".Trim() : "—",
        Email = c.User?.Email,
        IsActive = c.User?.IsActive ?? false,
        StoreId = c.StoreId,
        CreateDate = c.CreateDate,
        Street = c.User?.Address?.Street,
        PostalCode = c.User?.Address?.PostalCode,
        Phone = c.User?.Address?.Phone,
        CityName = c.User?.Address?.City?.Name,
        CountryName = c.User?.Address?.City?.Country?.Name
    };

    #endregion

    #region Inventory Mappings

    public static InventoryResponseDto ToResponseDto(this Inventory i) => new()
    {
        InventoryId = i.InventoryId,
        MovieId = i.MovieId,
        MovieTitle = i.Movie?.Title ?? "",
        StoreId = i.StoreId,
        IsAvailable = i.Rentals == null || !i.Rentals.Any(r => r.ReturnDate == null),
        LastUpdate = i.LastUpdate
    };

    public static InventoryDetailDto ToDetailDto(this Inventory i) => new()
    {
        InventoryId = i.InventoryId,
        MovieId = i.MovieId,
        MovieTitle = i.Movie?.Title ?? "",
        StoreId = i.StoreId,
        IsAvailable = i.Rentals == null || !i.Rentals.Any(r => r.ReturnDate == null),
        TotalRentals = i.Rentals?.Count ?? 0,
        LastUpdate = i.LastUpdate
    };

    #endregion

    #region Rental Mappings

    public static RentalResponseDto ToResponseDto(this Rental r) => new()
    {
        RentalId = r.RentalId,
        RentalDate = r.RentalDate,
        ReturnDate = r.ReturnDate,
        InventoryId = r.InventoryId,
        MovieId = r.Inventory?.MovieId ?? 0,
        MovieTitle = r.Inventory?.Movie?.Title ?? "",
        CustomerId = r.CustomerId,
        CustomerName = r.Customer?.User != null
            ? $"{r.Customer.User.FirstName} {r.Customer.User.LastName}".Trim()
            : $"Customer {r.CustomerId}",
        StaffId = r.StaffId,
        StaffName = r.Staff?.User != null
            ? $"{r.Staff.User.FirstName} {r.Staff.User.LastName}".Trim()
            : $"Staff {r.StaffId}",
        LastUpdate = r.LastUpdate,
        RentalRate = r.Inventory?.Movie?.RentalRate ?? 0,
        SuggestedAmount = r.Inventory?.Movie?.RentalRate ?? 0
    };

    public static RentalDetailDto ToDetailDto(this Rental r) => new()
    {
        RentalId = r.RentalId,
        RentalDate = r.RentalDate,
        ReturnDate = r.ReturnDate,
        InventoryId = r.InventoryId,
        MovieId = r.Inventory?.MovieId ?? 0,
        MovieTitle = r.Inventory?.Movie?.Title ?? "",
        CustomerId = r.CustomerId,
        CustomerName = r.Customer?.User != null
            ? $"{r.Customer.User.FirstName} {r.Customer.User.LastName}".Trim()
            : $"Customer {r.CustomerId}",
        StaffId = r.StaffId,
        StaffName = r.Staff?.User != null
            ? $"{r.Staff.User.FirstName} {r.Staff.User.LastName}".Trim()
            : $"Staff {r.StaffId}",
        TotalPaid = r.Payments?.Sum(p => p.Amount) ?? 0,
        PaymentCount = r.Payments?.Count ?? 0,
        LastUpdate = r.LastUpdate
    };

    #endregion

    #region Payment Mappings

    public static PaymentResponseDto ToResponseDto(this Payment p) => new()
    {
        PaymentId = p.PaymentId,
        CustomerId = p.CustomerId,
        CustomerName = p.Customer?.User != null
            ? $"{p.Customer.User.FirstName} {p.Customer.User.LastName}".Trim()
            : $"Customer {p.CustomerId}",
        StaffId = p.StaffId,
        StaffName = p.Staff?.User != null
            ? $"{p.Staff.User.FirstName} {p.Staff.User.LastName}".Trim()
            : $"Staff {p.StaffId}",
        RentalId = p.RentalId,
        MovieTitle = p.Rental?.Inventory?.Movie?.Title ?? "",
        Amount = p.Amount,
        PaymentDate = p.PaymentDate
    };

    #endregion

    #region Role Mappings

    public static RoleResponseDto ToResponseDto(this Role r) => new()
    {
        RoleId = r.RoleId,
        RoleName = r.RoleName,
        CreatedAt = r.CreatedAt
    };

    #endregion

    #region Store Mappings

    public static StoreResponseDto ToResponseDto(this Store s) => new()
    {
        StoreId = s.StoreId,
        ManagerStaffId = s.ManagerStaffId,
        ManagerName = s.ManagerStaff?.User != null
            ? $"{s.ManagerStaff.User.FirstName} {s.ManagerStaff.User.LastName}".Trim()
            : (s.ManagerStaffId > 0 ? $"Staff #{s.ManagerStaffId}" : "Unassigned"),
        Street = s.Address?.Street ?? "",
        PostalCode = s.Address?.PostalCode,
        Phone = s.Address?.Phone ?? "",
        CityName = s.Address?.City?.Name ?? "",
        CountryName = s.Address?.City?.Country?.Name ?? "",
        TotalStaff = s.Staff?.Count ?? 0,
        TotalCustomers = s.Customers?.Count ?? 0,
        TotalInventory = s.Inventories?.Count ?? 0
    };

    public static StoreDetailDto ToDetailDto(this Store s) => new()
    {
        StoreId = s.StoreId,
        ManagerStaffId = s.ManagerStaffId,
        ManagerName = s.ManagerStaff?.User != null
            ? $"{s.ManagerStaff.User.FirstName} {s.ManagerStaff.User.LastName}".Trim()
            : (s.ManagerStaffId > 0 ? $"Staff #{s.ManagerStaffId}" : "Unassigned"),
        AddressId = s.AddressId,
        Street = s.Address?.Street ?? "",
        PostalCode = s.Address?.PostalCode,
        Phone = s.Address?.Phone ?? "",
        CityName = s.Address?.City?.Name ?? "",
        CountryName = s.Address?.City?.Country?.Name ?? "",
        TotalStaff = s.Staff?.Count ?? 0,
        TotalCustomers = s.Customers?.Count ?? 0,
        TotalInventory = s.Inventories?.Count ?? 0,
        LastUpdate = s.LastUpdate
    };

    #endregion
}
