using PackageDeliverySystem.Core;

namespace PackageDeliverySystem.Core
{
    public interface IPackageRepository
    {
        Task<Package?> GetByIdAsync(int id);
        Task AddAsync(Package package);
        Task UpdateAsync(Package package);
        Task<IEnumerable<Package>> GetAllAsync();
        Task<IEnumerable<Package>> GetPackagesRequiringExpirationAsync();
        Task<IEnumerable<Package>> GetPackagesWithMissedDepositAsync();
    }
}
