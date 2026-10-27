using PackageDeliverySystem.Core;

namespace PackageDeliverySystem.Infrastructure;

public class PackageOrderService : IPackageOrderService
{
    private readonly IPackageRepository _packageRepository;
    private readonly IBoxRepository _boxRepository;
    private readonly IPeopleService _peopleService;

    public PackageOrderService(
        IPackageRepository packageRepository, IBoxRepository boxRepository, IPeopleService peopleService)
    {
        _packageRepository = packageRepository;
        _boxRepository = boxRepository;
        _peopleService = peopleService;
    }

    public async Task<Package> CreatePackageAsync(
        int senderId, int recipientId, int originLockerId, int destinationLockerId, BoxSize size)
    {
        var sender = await _peopleService.GetUserAsync(senderId)
            ?? throw new ArgumentException("Sender not found");
        var recipient = await _peopleService.GetUserAsync(recipientId)
            ?? throw new ArgumentException("Recipient not found");

        var originBox = await _boxRepository.FindFreeBoxAsync(originLockerId, size)
            ?? throw new InvalidOperationException(
                $"No free {size} box at the chosen origin locker - pick another locker or size");

        // excludeBoxId stops the same physical box being picked as both origin and
        // destination, which matters if the sender and recipient use the same locker
        var destinationBox = await _boxRepository.FindFreeBoxAsync(destinationLockerId, size, originBox.Id)
            ?? throw new InvalidOperationException(
                $"No free {size} box at the chosen destination locker - pick another locker or size");

        var package = new Package
        {
            Sender = sender,
            Recipient = recipient,
            Size = size,
            BoxId = originBox.Id,
            DestinationBoxId = destinationBox.Id
        };

        await _packageRepository.AddAsync(package);
        return package;
    }
}
