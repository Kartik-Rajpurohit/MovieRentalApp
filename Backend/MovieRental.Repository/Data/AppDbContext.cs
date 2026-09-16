using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Data
{
    // Main database context — registers all DbSets and applies entity configurations
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Application-level tables
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        // Location tables
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Address> Addresses { get; set; }

        // Movie-related tables
        public DbSet<Language> Languages { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieActor> MovieActors { get; set; }
        public DbSet<MovieCategory> MovieCategories { get; set; }

        // Business tables
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Automatically discovers and applies all IEntityTypeConfiguration classes in this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Configure default values for is_deleted column across all soft-deletable entities
            modelBuilder.Entity<Actor>().Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            modelBuilder.Entity<Address>().Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            modelBuilder.Entity<Category>().Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            modelBuilder.Entity<City>().Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            modelBuilder.Entity<Country>().Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            modelBuilder.Entity<Customer>().Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            modelBuilder.Entity<Inventory>().Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            modelBuilder.Entity<Language>().Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            modelBuilder.Entity<Movie>().Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            modelBuilder.Entity<Payment>().Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            modelBuilder.Entity<Rental>().Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            modelBuilder.Entity<Role>().Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            modelBuilder.Entity<Staff>().Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            modelBuilder.Entity<Store>().Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            modelBuilder.Entity<User>().Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        }
    }
}
