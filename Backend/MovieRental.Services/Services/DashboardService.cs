using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Dashboard;
using MovieRental.Domain.DTOs.Rentals;
using MovieRental.Domain.DTOs.Payments;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services;

public class DashboardService : IDashboardService
{
    private readonly IUserRepository _userRepository;
    private readonly IFilmRepository _filmRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IStaffRepository _staffRepository;

    public DashboardService(
        IUserRepository userRepository,
        IFilmRepository filmRepository,
        IRentalRepository rentalRepository,
        IPaymentRepository paymentRepository,
        IInventoryRepository inventoryRepository,
        ICustomerRepository customerRepository,
        IStaffRepository staffRepository)
    {
        _userRepository = userRepository;
        _filmRepository = filmRepository;
        _rentalRepository = rentalRepository;
        _paymentRepository = paymentRepository;
        _inventoryRepository = inventoryRepository;
        _customerRepository = customerRepository;
        _staffRepository = staffRepository;
    }

    public async Task<AdminDashboardDto> GetAdminDashboardAsync()
    {
        // Sequential awaits — EF Core DbContext is not thread-safe
        var totalUsers = await _userRepository.GetAllUsers().CountAsync();
        var totalFilms = await _filmRepository.GetAllFilms().CountAsync();
        var totalRentals = await _rentalRepository.GetAllRentals().CountAsync();
        var totalRevenue = await _paymentRepository.GetAllPayments().SumAsync(p => p.Amount);
        var activeRentals = await _rentalRepository.GetAllRentals().CountAsync(r => r.ReturnDate == null);
        var availableInventory = await _inventoryRepository.GetAllInventory().CountAsync(i => !i.Rentals.Any(r => r.ReturnDate == null));
        var totalCustomers = await _customerRepository.GetAllCustomers().CountAsync();
        var totalStaff = await _staffRepository.GetAllStaff().CountAsync();

        var topFilms = await _filmRepository.GetAllFilms()
            .Select(f => new TopFilmDto
            {
                FilmId = f.FilmId,
                Title = f.Title,
                RentalCount = f.Inventories.SelectMany(i => i.Rentals).Count()
            })
            .OrderByDescending(f => f.RentalCount)
            .Take(5)
            .ToListAsync();

        var revenueByStore = await _paymentRepository.GetAllPayments()
            .GroupBy(p => p.Staff.StoreId)
            .Select(g => new StoreRevenueDto
            {
                StoreId = g.Key,
                StoreName = $"Store #{g.Key}",
                Revenue = g.Sum(p => p.Amount)
            })
            .ToListAsync();

        var recentRentals = await _rentalRepository.GetAllRentals()
            .OrderByDescending(r => r.RentalId)
            .Take(5)
            .Select(r => new RentalResponseDto
            {
                RentalId = r.RentalId,
                RentalDate = r.RentalDate,
                ReturnDate = r.ReturnDate,
                FilmTitle = r.Inventory.Film.Title,
                CustomerName = r.Customer.User.FirstName + " " + r.Customer.User.LastName,
                StaffName = r.Staff.User.FirstName + " " + r.Staff.User.LastName,
                RentalRate = r.Inventory.Film.RentalRate,
                SuggestedAmount = r.ReturnDate.HasValue
                    ? r.Inventory.Film.RentalRate * (decimal)Math.Max(1, (r.ReturnDate.Value - r.RentalDate).TotalDays)
                    : r.Inventory.Film.RentalRate,
            })
            .ToListAsync();

        return new AdminDashboardDto
        {
            TotalUsers = totalUsers,
            TotalFilms = totalFilms,
            TotalRentals = totalRentals,
            TotalRevenue = totalRevenue,
            ActiveRentals = activeRentals,
            AvailableInventory = availableInventory,
            TotalCustomers = totalCustomers,
            TotalStaff = totalStaff,
            TopRentedFilms = topFilms,
            RevenueByStore = revenueByStore,
            RecentRentals = recentRentals,
        };
    }

    public async Task<StaffDashboardDto> GetStaffDashboardAsync(int userId)
    {
        var staff = await _staffRepository.GetAllStaff()
            .FirstOrDefaultAsync(s => s.UserId == userId);
        var storeId = staff?.StoreId ?? 0;
        var today = DateTime.UtcNow.Date;

        var activeRentals = await _rentalRepository.GetAllRentals()
            .CountAsync(r => r.Staff.StoreId == storeId && r.ReturnDate == null);
        var availableInventory = await _inventoryRepository.GetAllInventory()
            .CountAsync(i => i.StoreId == storeId && !i.Rentals.Any(r => r.ReturnDate == null));
        var totalCustomers = await _customerRepository.GetAllCustomers()
            .CountAsync(c => c.StoreId == storeId);
        var todaysPayments = await _paymentRepository.GetAllPayments()
            .Where(p => p.Staff.StoreId == storeId && p.PaymentDate.Date == today)
            .SumAsync(p => p.Amount);

        var recentRentals = await _rentalRepository.GetAllRentals()
            .Where(r => r.Staff.StoreId == storeId)
            .OrderByDescending(r => r.RentalId)
            .Take(5)
            .Select(r => new RentalResponseDto
            {
                RentalId = r.RentalId,
                RentalDate = r.RentalDate,
                ReturnDate = r.ReturnDate,
                FilmTitle = r.Inventory.Film.Title,
                CustomerName = r.Customer.User.FirstName + " " + r.Customer.User.LastName,
                StaffName = r.Staff.User.FirstName + " " + r.Staff.User.LastName,
                RentalRate = r.Inventory.Film.RentalRate,
            })
            .ToListAsync();

        var recentPayments = await _paymentRepository.GetAllPayments()
            .Where(p => p.Staff.StoreId == storeId)
            .OrderByDescending(p => p.PaymentId)
            .Take(5)
            .Select(p => new PaymentResponseDto
            {
                PaymentId = p.PaymentId,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                CustomerName = p.Customer.User.FirstName + " " + p.Customer.User.LastName,
                FilmTitle = p.Rental.Inventory.Film.Title,
            })
            .ToListAsync();

        return new StaffDashboardDto
        {
            StoreId = storeId,
            ActiveRentals = activeRentals,
            AvailableInventory = availableInventory,
            TotalCustomers = totalCustomers,
            TodaysPayments = todaysPayments,
            RecentRentals = recentRentals,
            RecentPayments = recentPayments,
        };
    }

    public async Task<CustomerDashboardDto> GetCustomerDashboardAsync(int userId)
    {
        var customer = await _customerRepository.GetAllCustomers()
            .FirstOrDefaultAsync(c => c.UserId == userId);
        var customerId = customer?.CustomerId ?? 0;

        var activeRentals = await _rentalRepository.GetAllRentals()
            .CountAsync(r => r.CustomerId == customerId && r.ReturnDate == null);
        var totalRentals = await _rentalRepository.GetAllRentals()
            .CountAsync(r => r.CustomerId == customerId);
        var totalSpent = await _paymentRepository.GetAllPayments()
            .Where(p => p.CustomerId == customerId)
            .SumAsync(p => p.Amount);

        var myActiveRentals = await _rentalRepository.GetAllRentals()
            .Where(r => r.CustomerId == customerId && r.ReturnDate == null)
            .OrderByDescending(r => r.RentalId)
            .Take(10)
            .Select(r => new RentalResponseDto
            {
                RentalId = r.RentalId,
                RentalDate = r.RentalDate,
                ReturnDate = r.ReturnDate,
                FilmTitle = r.Inventory.Film.Title,
                CustomerName = r.Customer.User.FirstName + " " + r.Customer.User.LastName,
                StaffName = r.Staff.User.FirstName + " " + r.Staff.User.LastName,
                RentalRate = r.Inventory.Film.RentalRate,
                SuggestedAmount = r.Inventory.Film.RentalRate,
            })
            .ToListAsync();

        return new CustomerDashboardDto
        {
            ActiveRentals = activeRentals,
            TotalRentals = totalRentals,
            TotalSpent = totalSpent,
            MyActiveRentals = myActiveRentals,
        };
    }
}