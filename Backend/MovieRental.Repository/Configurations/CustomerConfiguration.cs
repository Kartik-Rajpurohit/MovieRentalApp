using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Configurations
{
    // Configuration for Customer entity — defines relationships with Store and User
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            // Many Customers → One Store (registered store)
            builder.HasOne(c => c.Store)
                .WithMany(s => s.Customers)
                .HasForeignKey(c => c.StoreId);

            // One Customer → One User account (optional — FK is on Customer table)
            builder.HasOne(c => c.User)
                .WithOne(u => u.Customer)
                .HasForeignKey<Customer>(c => c.UserId)
                .IsRequired(false);
        }
    }
}
