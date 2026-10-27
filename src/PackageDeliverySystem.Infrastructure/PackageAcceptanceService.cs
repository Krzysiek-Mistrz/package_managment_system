namespace PackageDeliverySystem.Core;

/// <summary>
/// service 4 orchestration user lookup, the package state change &amp; the expiration check
/// </summary>
public class PackageAcceptanceService : IPackageAcceptanceService
{
    private readonly IPackageRepository _packageRepository;
    private readonly IBoxRepository _boxRepository;
    private readonly INotificationService _notificationService;
    private readonly IPeopleService _peopleService;

    public PackageAcceptanceService(
        IPackageRepository packageRepository,
        IBoxRepository boxRepository,
        INotificationService notificationService,
        IPeopleService peopleService)
    {
        _packageRepository = packageRepository;
        _boxRepository = boxRepository;
        _notificationService = notificationService;
        _peopleService = peopleService;
    }

    /// <summary>
    /// accepts a package and updates its status
    /// </summary>
    public async Task AcceptPackageAsync(int packageId, int operatorId, int boxId)
    {
        var package = await _packageRepository.GetByIdAsync(packageId);
        
        if (package == null)
            throw new ArgumentException("Package not found");
        if (package.Status != PackageStatus.Ordered)
            throw new InvalidOperationException("Package is not in an orderable state");
        if (package.BoxId != boxId)
            throw new ArgumentException("Box ID does not match the package's assigned box");

        var box = await _boxRepository.GetByIdAsync(boxId)
            ?? throw new ArgumentException("Box not found");
        if (box.Size != package.Size)
        {
            throw new InvalidOperationException(
                $"Package declared as {package.Size} cannot be accepted into a {box.Size} box");
        }

        var operatorUser = await _peopleService.GetOperatorAsync(operatorId);

        if (operatorUser == null)
        {
            throw new ArgumentException("Operator not found");
        }

        package.Accept();
        package.AssignOperator(operatorUser);
        
        await _packageRepository.UpdateAsync(package);
        
        await _notificationService.NotifyAsync("system", 
            $"Package {package.Id} has been accepted by operator {operatorUser.Name}.");
    }

    /// <summary>
    /// rejects a package (e.g. damaged on inspection, suspicious contents). allowed from
    /// Ordered or Accepted - same states Package.Reject() itself allows. doesn't call
    /// AssignOperator, since that requires Status == Accepted and this can run from Ordered too;
    /// the operator's name only goes into the notification, not onto the package record
    /// </summary>
    public async Task RejectPackageAsync(int packageId, int operatorId)
    {
        var package = await _packageRepository.GetByIdAsync(packageId)
            ?? throw new ArgumentException("Package not found");

        var operatorUser = await _peopleService.GetOperatorAsync(operatorId)
            ?? throw new ArgumentException("Operator not found");

        package.Reject();
        await _packageRepository.UpdateAsync(package);

        await _notificationService.NotifyAsync("system",
            $"Package {package.Id} has been rejected by operator {operatorUser.Name}.");
    }

    /// <summary>
    /// scans 4 packages that have exceeded their allowed time in a certain state
    /// </summary>
    public async Task ProcessExpiredPackagesAsync()
    {
        var expiredPackages = await _packageRepository.GetPackagesRequiringExpirationAsync();

        foreach (var package in expiredPackages)
        {
            try 
            {
                package.Expire();
                await _packageRepository.UpdateAsync(package);
                
                await _notificationService.NotifyAsync("system", 
                    $"Package {package.Id} has been marked as EXPIRED due to inactivity.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error expiring package {package.Id}: {ex.Message}");
            }
        }
    }
}
