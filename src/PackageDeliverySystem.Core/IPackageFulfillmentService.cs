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

    /// <summary>
    /// userId must be the package's own recipient, and code must match the pickup code
    /// generated at Deliver() - anyone who just knows a packageId cannot collect it
    /// </summary>
    Task CollectPackageAsync(int packageId, int userId, string code);
}
