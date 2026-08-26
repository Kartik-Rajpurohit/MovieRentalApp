using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Configurations
{
    // Configuration for Inventory entity — defines relationships with Film and Store
    public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
    {
        public void Configure(EntityTypeBuilder<Inventory> builder)
        {
            // Many Inventory copies → One Film
            builder.HasOne(i => i.Film)
                .WithMany(f => f.Inventories)
                .HasForeignKey(i => i.FilmId);

            // Many Inventory copies → One Store
            builder.HasOne(i => i.Store)
                .WithMany(s => s.Inventories)
                .HasForeignKey(i => i.StoreId);
        }
    }
}
