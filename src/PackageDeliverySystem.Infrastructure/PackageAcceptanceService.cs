namespace PackageDeliverySystem.Core;

/// <summary>
/// service 4 orchestration user lookup, the package state change & the expiration check
/// </summary>
public class PackageAcceptanceService : IPackageAcceptanceService
{
    private readonly IPackageRepository _packageRepository;
    private readonly INotificationService _notificationService;
    private readonly IPeopleService _peopleService;

    public PackageAcceptanceService(IPackageRepository packageRepository, INotificationService notificationService)
    {
        _packageRepository = packageRepository;
        _notificationService = notificationService;
        _peopleService = new PeopleService();
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

        var operatorUser = await _peopleService.GetOperatorAsync(operatorId);

        if (operatorUser == null)
            throw new ArgumentException("Operator not found");

        package.Accept();
        package.AssignOperator(operatorUser);
        
        await _packageRepository.UpdateAsync(package);
        
        await _notificationService.NotifyAsync("system", 
            $"Package {package.Id} has been accepted by operator {operatorUser.Name}.");
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
