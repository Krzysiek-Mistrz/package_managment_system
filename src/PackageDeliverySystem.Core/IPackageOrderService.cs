namespace PackageDeliverySystem.Core;

/// <summary>
/// creates a new order. finds a free box of the requested size at both the origin and the
/// destination locker before the Package even exists, so a customer is never handed a
/// reservation the system can't actually fulfil
/// </summary>
public interface IPackageOrderService
{
    Task<Package> CreatePackageAsync(
        int senderId, int recipientId, int originLockerId, int destinationLockerId, BoxSize size);
}
