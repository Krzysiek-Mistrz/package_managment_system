using PackageDeliverySystem.Core;

namespace PackageDeliverySystem.Infrastructure;

/// <summary>
/// implements the post-acceptance package lifecycle: assigning a courier, shipping,
/// delivering and collecting. mirrors PackageAcceptanceService's shape - look up the
/// package (and any other required entity), call the matching domain method, persist, notify
/// </summary>
public class PackageFulfillmentService : IPackageFulfillmentService
{
    private readonly IPackageRepository _packageRepository;
    private readonly INotificationService _notificationService;
    private readonly IPeopleService _peopleService;

    public PackageFulfillmentService(
        IPackageRepository packageRepository,
        INotificationService notificationService,
        IPeopleService peopleService)
    {
        _packageRepository = packageRepository;
        _notificationService = notificationService;
        _peopleService = peopleService;
    }

    public async Task AssignCourierAsync(int packageId, int courierId)
    {
        var package = await GetPackageOrThrowAsync(packageId);
        var courier = await _peopleService.GetCourierAsync(courierId)
            ?? throw new ArgumentException("Courier not found");

        package.AssignCourier(courier);
        await _packageRepository.UpdateAsync(package);

        await _notificationService.NotifyAsync("system",
            $"Package {package.Id} has been assigned to courier {courier.Name}.");
    }

    public async Task ShipPackageAsync(int packageId)
    {
        var package = await GetPackageOrThrowAsync(packageId);

        package.Prepare();
        package.Ship();
        await _packageRepository.UpdateAsync(package);

        await _notificationService.NotifyAsync("system",
            $"Package {package.Id} has been shipped.");
    }

    public async Task DeliverPackageAsync(int packageId)
    {
        var package = await GetPackageOrThrowAsync(packageId);

        package.Deliver();
        await _packageRepository.UpdateAsync(package);

        await _notificationService.NotifyAsync("system",
            $"Package {package.Id} has been delivered.");
    }

    public async Task CollectPackageAsync(int packageId)
    {
        var package = await GetPackageOrThrowAsync(packageId);

        package.Collect();
        await _packageRepository.UpdateAsync(package);

        await _notificationService.NotifyAsync("system",
            $"Package {package.Id} has been collected.");
    }

    private async Task<Package> GetPackageOrThrowAsync(int packageId)
        => await _packageRepository.GetByIdAsync(packageId)
           ?? throw new ArgumentException("Package not found");
}
