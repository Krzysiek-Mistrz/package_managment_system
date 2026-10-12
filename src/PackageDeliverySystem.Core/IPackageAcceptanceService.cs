namespace PackageDeliverySystem.Core;

public interface IPackageAcceptanceService
{
    Task AcceptPackageAsync(int packageId, int operatorId, int boxId);
    Task ProcessExpiredPackagesAsync();
}