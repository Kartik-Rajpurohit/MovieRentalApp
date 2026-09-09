using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    // Defines database operations for rental Payment transactions.
    public interface IPaymentRepository
    {
        // Returns a queryable collection of payments with customer, staff, and rental relations.
        IQueryable<Payment> GetAllPayments();

        // Finds a payment transaction by its ID.
        Task<Payment?> GetPaymentByIdAsync(int id);

        // Adds a new payment record to the database ledger.
        Task<Payment> CreatePaymentAsync(Payment payment);
    }
}
