using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    // Handles database operations related to rental payment transactions.
    public class PaymentRepository : IPaymentRepository
    {
        // Receives the database context used to access payment and ledger tables.
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        // Reads all payment records without tracking, including Customer, Staff, Rental, Inventory, and Film.
        public IQueryable<Payment> GetAllPayments()
            => _context.Payments
                .AsNoTracking()
                .Include(p => p.Customer).ThenInclude(c => c.User)
                .Include(p => p.Staff).ThenInclude(s => s.User)
                .Include(p => p.Rental).ThenInclude(r => r.Inventory).ThenInclude(i => i.Film)
                .AsQueryable();

        // Finds a payment transaction by ID with full relation trees.
        public async Task<Payment?> GetPaymentByIdAsync(int id)
            => await _context.Payments
                .Include(p => p.Customer).ThenInclude(c => c.User)
                .Include(p => p.Staff).ThenInclude(s => s.User)
                .Include(p => p.Rental).ThenInclude(r => r.Inventory).ThenInclude(i => i.Film)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

        // Inserts a new payment record and re-fetches it with full navigation properties.
        public async Task<Payment> CreatePaymentAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return await GetPaymentByIdAsync(payment.PaymentId) ?? payment;
        }
    }
}
