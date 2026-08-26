using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Configurations
{
    // Configuration for Store entity — defines relationships with Address and ManagerStaff
    public class StoreConfiguration : IEntityTypeConfiguration<Store>
    {
        public void Configure(EntityTypeBuilder<Store> builder)
        {
            // Many Stores → One Address
            builder.HasOne(s => s.Address)
                .WithMany(a => a.Stores)
                .HasForeignKey(s => s.AddressId);

            // Each Store has one Manager who is a Staff member
            // WithMany() is empty because Staff does not track managed stores
            builder.HasOne(s => s.ManagerStaff)
                .WithMany()
                .HasForeignKey(s => s.ManagerStaffId);
        }
    }
}
