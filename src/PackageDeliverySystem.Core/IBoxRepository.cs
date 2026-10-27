namespace PackageDeliverySystem.Core;

public interface IBoxRepository
{
    Task<Box?> GetByIdAsync(int boxId);

    /// <summary>
    /// finds a box of the given size at the given locker that isn't currently tied up by
    /// another package (as either its origin or its destination). excludeBoxId lets a caller
    /// rule out a box it already picked for something else, so origin and destination never
    /// end up pointing at the same physical box
    /// </summary>
    Task<Box?> FindFreeBoxAsync(int parcelLockerId, BoxSize size, int? excludeBoxId = null);
}
