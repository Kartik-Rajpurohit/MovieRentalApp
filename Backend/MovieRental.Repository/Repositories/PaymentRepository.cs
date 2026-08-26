using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<Payment> GetAllPayments()
            => _context.Payments
                .Include(p => p.Customer).ThenInclude(c => c.User)
                .Include(p => p.Staff).ThenInclude(s => s.User)
                .Include(p => p.Rental).ThenInclude(r => r.Inventory).ThenInclude(i => i.Film)
                .AsQueryable();

        public async Task<Payment?> GetPaymentByIdAsync(int id)
            => await _context.Payments
                .Include(p => p.Customer).ThenInclude(c => c.User)
                .Include(p => p.Staff).ThenInclude(s => s.User)
                .Include(p => p.Rental).ThenInclude(r => r.Inventory).ThenInclude(i => i.Film)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

        public async Task<Payment> CreatePaymentAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return await GetPaymentByIdAsync(payment.PaymentId) ?? payment;
        }
    }
}
