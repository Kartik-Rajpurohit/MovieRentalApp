using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Configurations
{
    // Configuration for Staff entity — defines relationships with Store and User
    public class StaffConfiguration : IEntityTypeConfiguration<Staff>
    {
        public void Configure(EntityTypeBuilder<Staff> builder)
        {
            // Many Staff → One Store (store where staff works)
            builder.HasOne(s => s.Store)
                .WithMany(s => s.Staff)
                .HasForeignKey(s => s.StoreId);

            // One Staff → One User account (optional — FK is on Staff table)
            builder.HasOne(s => s.User)
                .WithOne(u => u.Staff)
                .HasForeignKey<Staff>(s => s.UserId)
                .IsRequired(false);
        }
    }
}
