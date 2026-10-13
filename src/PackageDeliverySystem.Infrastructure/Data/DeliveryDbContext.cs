using Microsoft.EntityFrameworkCore;
using PackageDeliverySystem.Core;

namespace PackageDeliverySystem.Infrastructure.Data
{
    public class DeliveryDbContext : DbContext
    {
        public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options) { }
        // tables   
        public DbSet<Package> Packages { get; set; }
        public DbSet<Box> Boxes { get; set; }
        public DbSet<ParcelLocker> ParcelLockers { get; set; }
        public DbSet<AbstractPerson> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Package>().HasKey(p => p.Id);
            modelBuilder.Entity<Box>().HasKey(b => b.Id);
            modelBuilder.Entity<ParcelLocker>().HasKey(pl => pl.Id);
            modelBuilder.Entity<AbstractPerson>().HasKey(u => u.Id);
            modelBuilder.Entity<AbstractPerson>()
                .HasDiscriminator<string>("UserType")
                .HasValue<Operator>("Operator")
                .HasValue<Courier>("Courier")
                .HasValue<User>("User");

            // relationships
            modelBuilder.Entity<Box>()
                .HasMany(b => b.Packages)
                .WithOne(p => p.Box)
                .HasForeignKey(p => p.BoxId);

            modelBuilder.Entity<ParcelLocker>()
                .HasMany(pl => pl.Boxes)
                .WithOne(b => b.ParcelLocker)
                .HasForeignKey(b => b.ParcelLockerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}