namespace PackageDeliverySystem.Core;

public interface IBoxRepository
{
    Task<Box?> GetByIdAsync(int boxId);
}
