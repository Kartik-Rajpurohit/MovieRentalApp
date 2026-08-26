using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    public interface IPaymentRepository
    {
        IQueryable<Payment> GetAllPayments();
        Task<Payment?> GetPaymentByIdAsync(int id);
        Task<Payment> CreatePaymentAsync(Payment payment);
    }
}
