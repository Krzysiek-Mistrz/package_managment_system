using PackageDeliverySystem.Core;

namespace PackageDeliverySystem.Infrastructure;

/// <summary>
/// orchestrates payment and physical deposit confirmation before a package can be accepted.
/// the real payment provider is injected as IPaymentGateway - this class never talks to a
/// payment api directly, so swapping providers never touches this code (open/closed principle)
/// </summary>
public class PackageIntakeService : IPackageIntakeService
{
    private readonly IPackageRepository _packageRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly INotificationService _notificationService;
    private readonly IPeopleService _peopleService;

    public PackageIntakeService(
        IPackageRepository packageRepository,
        IPaymentGateway paymentGateway,
        INotificationService notificationService,
        IPeopleService peopleService)
    {
        _packageRepository = packageRepository;
        _paymentGateway = paymentGateway;
        _notificationService = notificationService;
        _peopleService = peopleService;
    }

    /// <summary>
    /// starts a payment with the external gateway and records it as Pending.
    /// edge case: refuses to start a second payment once one has already completed,
    /// but allows re-requesting after a Failed attempt or a still-Pending one
    /// </summary>
    public async Task<string> RequestPaymentAsync(int packageId, string payerEmail, decimal amount)
    {
        var package = await GetPackageOrThrowAsync(packageId);

        if (package.Payment is { Status: PaymentStatus.Completed })
            throw new InvalidOperationException("This package has already been paid for.");

        var externalReference = await _paymentGateway.InitiatePaymentAsync(payerEmail, amount);

        package.Payment = new Payment
        {
            PackageId = package.Id,
            PayerEmail = payerEmail,
            Amount = amount,
            ExternalReference = externalReference
        };
        await _packageRepository.UpdateAsync(package);

        return externalReference;
    }

    /// <summary>
    /// asks the gateway for the current outcome and applies it
    /// </summary>
    public async Task ConfirmPaymentAsync(int packageId)
    {
        var package = await GetPackageOrThrowAsync(packageId);
        var payment = package.Payment
            ?? throw new InvalidOperationException("No payment was requested for this package.");

        if (payment.Status == PaymentStatus.Completed)
            return;

        var gatewayStatus = await _paymentGateway.GetPaymentStatusAsync(payment.ExternalReference!);

        if (gatewayStatus == PaymentStatus.Completed)
        {
            payment.Complete();
            await _packageRepository.UpdateAsync(package);
            await _notificationService.NotifyAsync("system",
                $"Payment for package {package.Id} has been confirmed.");
        }
        else if (gatewayStatus == PaymentStatus.Failed)
        {
            payment.Fail();
            await _packageRepository.UpdateAsync(package);
        }
    }

    /// <summary>
    /// the sender never self-reports this - only a courier who actually opened the box and
    /// looked inside can. nothing here detects the package automatically; this just records
    /// that fact once the courier's app calls it, after the courier confirms it by eye.
    ///
    /// courierId must come from the caller's authenticated session, never from client input -
    /// GetCourierAsync only proves this id belongs to *some* real courier, not that the
    /// current caller *is* that courier. enforcing that is the implementer's job (auth layer),
    /// since this framework has no login system of its own -> to be done by developer implementing this framework :)
    /// </summary>
    public async Task ConfirmDepositAsync(int packageId, int courierId)
    {
        var package = await GetPackageOrThrowAsync(packageId);
        var courier = await _peopleService.GetCourierAsync(courierId)
            ?? throw new ArgumentException("Courier not found");

        package.ConfirmDeposit();
        await _packageRepository.UpdateAsync(package);

        await _notificationService.NotifyAsync("system",
            $"Courier {courier.Name} confirmed package {package.Id} is in its box.");
    }

    /// <summary>
    /// edge case: the courier opened the box at pickup time and found nothing in it.
    /// cancels right away, on the spot - fault lies with the sender, not the system
    /// </summary>
    public async Task ReportMissingDepositAsync(int packageId, int courierId)
    {
        var package = await GetPackageOrThrowAsync(packageId);
        var courier = await _peopleService.GetCourierAsync(courierId)
            ?? throw new ArgumentException("Courier not found");

        package.Cancel();
        await _packageRepository.UpdateAsync(package);

        await _notificationService.NotifyAsync("system",
            $"Package {package.Id} was cancelled: courier {courier.Name} found the box empty at pickup (sender's fault).");
    }

    /// <summary>
    /// edge case: the sender never put anything in the reserved box. cancels the order and
    /// frees the reservation (box occupancy is status-driven elsewhere, so nothing else to clean up)
    /// </summary>
    public async Task ProcessMissedDepositsAsync()
    {
        var missed = await _packageRepository.GetPackagesWithMissedDepositAsync();

        foreach (var package in missed)
        {
            try
            {
                package.Cancel();
                await _packageRepository.UpdateAsync(package);

                await _notificationService.NotifyAsync("system",
                    $"Package {package.Id} was cancelled: nothing was deposited before the pickup window closed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cancelling package {package.Id}: {ex.Message}");
            }
        }
    }

    private async Task<Package> GetPackageOrThrowAsync(int packageId)
        => await _packageRepository.GetByIdAsync(packageId)
           ?? throw new ArgumentException("Package not found");
}
