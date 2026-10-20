namespace PackageDeliverySystem.Core;

/// <summary>
/// orchestrates the package lifecycle steps that happen after operator acceptance:
/// courier assignment, shipping, delivery and collection
/// </summary>
public interface IPackageFulfillmentService
{
    Task AssignCourierAsync(int packageId, int courierId);
    Task ShipPackageAsync(int packageId);
    Task DeliverPackageAsync(int packageId);
    Task CollectPackageAsync(int packageId);
}
