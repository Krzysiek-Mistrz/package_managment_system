using PackageDeliverySystem.Core;

namespace PackageDeliverySystem.Core
{
    public interface IPackageRepository
    {
        Task<Package?> GetByIdAsync(Guid id);
        Task AddAsync(Package package);
        Task UpdateAsync(Package package);
        Task<IEnumerable<Package>> GetAllAsync();
        Task<IEnumerable<Package>> GetPackagesRequiringExpirationAsync();
    }
}