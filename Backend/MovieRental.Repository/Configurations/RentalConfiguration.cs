using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Configurations
{
    // Configuration for Rental entity — defines relationships with Inventory, Customer and Staff
    public class RentalConfiguration : IEntityTypeConfiguration<Rental>
    {
        public void Configure(EntityTypeBuilder<Rental> builder)
        {
            // Many Rentals → One Inventory copy
            builder.HasOne(r => r.Inventory)
                .WithMany(i => i.Rentals)
                .HasForeignKey(r => r.InventoryId);

            // Many Rentals → One Customer
            builder.HasOne(r => r.Customer)
                .WithMany(c => c.Rentals)
                .HasForeignKey(r => r.CustomerId);

            // Many Rentals → One Staff (who processed the rental)
            builder.HasOne(r => r.Staff)
                .WithMany(s => s.Rentals)
                .HasForeignKey(r => r.StaffId);
        }
    }
}
