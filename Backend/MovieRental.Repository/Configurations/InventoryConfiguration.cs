using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Configurations
{
    // Configuration for Inventory entity — defines relationships with Movie and Store
    public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
    {
        public void Configure(EntityTypeBuilder<Inventory> builder)
        {
            // Many Inventory copies → One Movie
            builder.HasOne(i => i.Movie)
                .WithMany(m => m.Inventories)
                .HasForeignKey(i => i.MovieId);

            // Many Inventory copies → One Store
            builder.HasOne(i => i.Store)
                .WithMany(s => s.Inventories)
                .HasForeignKey(i => i.StoreId);

            // Indexes
            builder.HasIndex(i => new { i.MovieId, i.StoreId });
        }
    }
}
