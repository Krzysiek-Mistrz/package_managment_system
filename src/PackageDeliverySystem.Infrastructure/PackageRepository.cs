using Microsoft.EntityFrameworkCore;
using PackageDeliverySystem.Core;
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

        public async Task<Package?> GetByIdAsync(Guid id)
        {
            return await _context.Packages.FindAsync(id);
        }

        public async Task AddAsync(Package package)
        {
            await _context.Packages.AddAsync(package);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Package package)
        {
            _context.Packages.Update(package);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Package>> GetAllAsync()
        {
            return await _context.Packages.ToListAsync();
        }
    }
}