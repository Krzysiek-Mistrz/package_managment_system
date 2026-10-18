using Microsoft.EntityFrameworkCore;
using PackageDeliverySystem.Core;

namespace PackageDeliverySystem.Infrastructure.Data
{
    public class DeliveryDbContext : DbContext
    {
        public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options) { }

        public DbSet<Package> Packages { get; set; }
        public DbSet<Box> Boxes { get; set; }
        public DbSet<ParcelLocker> ParcelLockers { get; set; }
        public DbSet<AbstractPerson> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Package>().Property(p => p.Status).HasConversion<string>().HasMaxLength(30);
            modelBuilder.Entity<Box>().Property(b => b.Size).HasConversion<string>().HasMaxLength(30);

            modelBuilder.Entity<ParcelLocker>().Property(pl => pl.Country).HasMaxLength(100);
            modelBuilder.Entity<ParcelLocker>().Property(pl => pl.City).HasMaxLength(100);
            modelBuilder.Entity<ParcelLocker>().Property(pl => pl.PostalCode).HasMaxLength(30);
            modelBuilder.Entity<ParcelLocker>().Property(pl => pl.Street).HasMaxLength(100);
            modelBuilder.Entity<ParcelLocker>().Property(pl => pl.Number).HasMaxLength(20);

            modelBuilder.Entity<AbstractPerson>()
                .ToTable("Users")
                .HasDiscriminator<string>("Role")
                .HasValue<Operator>("Operator")
                .HasValue<Courier>("Courier")
                .HasValue<User>("Customer");
            modelBuilder.Entity<AbstractPerson>().Property("Role").HasMaxLength(20);
            modelBuilder.Entity<AbstractPerson>().Property(u => u.Name).HasMaxLength(60);
            modelBuilder.Entity<AbstractPerson>().Property(u => u.Surname).HasMaxLength(60);
            modelBuilder.Entity<AbstractPerson>().Property(u => u.Email).HasMaxLength(255);
            modelBuilder.Entity<AbstractPerson>().Property(u => u.PhoneNumber).HasMaxLength(20);

            modelBuilder.Entity<Package>()
                .HasOne(p => p.Sender)
                .WithMany(u => u.SentPackages)
                .HasForeignKey(p => p.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Package>()
                .HasOne(p => p.Recipient)
                .WithMany(u => u.ReceivedPackages)
                .HasForeignKey(p => p.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Package>()
                .HasOne(p => p.Courier)
                .WithMany(c => c.Packages)
                .HasForeignKey(p => p.CourierId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Package>()
                .HasOne(p => p.Operator)
                .WithMany(o => o.Packages)
                .HasForeignKey(p => p.OperatorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Box>()
                .HasMany(b => b.Packages)
                .WithOne(p => p.Box)
                .HasForeignKey(p => p.BoxId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ParcelLocker>()
                .HasMany(pl => pl.Boxes)
                .WithOne(b => b.ParcelLocker)
                .HasForeignKey(b => b.ParcelLockerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
