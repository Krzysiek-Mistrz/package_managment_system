namespace PackageDeliverySystem.Core;

/// <summary>
/// interface 4 screen methods 4 packagedeliverysystem.web blazor
/// </summary>
public interface IDeliveryDataService
{
    public Task<IEnumerable<Package>> GetAllPackages();
    public Task<IEnumerable<Package>> GetOrderedPackages();
    public Task<Package?> GetPackage(int id);
    public Task<IEnumerable<Package>> GetCourierPackages(int courierId);
    public Task<IEnumerable<ParcelLocker>> GetParcelLockers();
    public Task AddPackage(Package package);
}