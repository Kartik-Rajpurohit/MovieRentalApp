using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Configurations
{
    // Configuration for Payment entity — defines relationships with Customer, Staff and Rental
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            // Many Payments → One Customer
            builder.HasOne(p => p.Customer)
                .WithMany(c => c.Payments)
                .HasForeignKey(p => p.CustomerId);

            // Many Payments → One Staff (who collected the payment)
            builder.HasOne(p => p.Staff)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.StaffId);

            // Many Payments → One Rental
            builder.HasOne(p => p.Rental)
                .WithMany(r => r.Payments)
                .HasForeignKey(p => p.RentalId);
        }
    }
}
