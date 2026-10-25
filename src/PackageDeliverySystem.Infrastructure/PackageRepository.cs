using Microsoft.EntityFrameworkCore;
using PackageDeliverySystem.Core;
using PackageDeliverySystem.Infrastructure.Data;

namespace PackageDeliverySystem.Infrastructure
{
    public class PackageRepository : IPackageRepository
    {
        private readonly DeliveryDbContext _context;

        public PackageRepository(DeliveryDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// retrieves a package by its unique identifier, including its payment -
        /// Package.Accept() needs the payment loaded to check it was completed
        /// </summary>
        public async Task<Package?> GetByIdAsync(int id)
        {
            return await _context.Packages
                .Include(p => p.Payment)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// adds a new package to the repository
        /// </summary>
        public async Task AddAsync(Package package)
        {
            await _context.Packages.AddAsync(package);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// updates an existing package in the repository
        /// </summary>
        public async Task UpdateAsync(Package package)
        {
            _context.Packages.Update(package);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// retrieves all packages from the repository
        /// </summary>
        public async Task<IEnumerable<Package>> GetAllAsync()
        {
            return await _context.Packages.ToListAsync();
        }

        /// <summary>
        /// scans 4 packages that have exceeded their allowed time 
        /// </summary>
        public async Task<IEnumerable<Package>> GetPackagesRequiringExpirationAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Packages
                .Where(p => 
                    (p.Status == PackageStatus.Accepted && p.ExpiredAt < now) ||
                    (p.Status == PackageStatus.Shipped && p.ExpiredAt < now)
                )
                .ToListAsync();
        }

        /// <summary>
        /// scans 4 packages whose deposit window closed without the sender ever confirming
        /// they put the item in the box
        /// </summary>
        public async Task<IEnumerable<Package>> GetPackagesWithMissedDepositAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Packages
                .Where(p =>
                    p.Status == PackageStatus.Ordered &&
                    p.DepositConfirmedAt == null &&
                    p.DepositDeadline < now
                )
                .ToListAsync();
        }
    }
}
