namespace PackageDeliverySystem.Core;

/// <summary>
/// orchestrates everything that must happen between a customer placing an order and an
/// operator accepting it: requesting and confirming payment, confirming the physical
/// deposit into the origin box, and releasing orders where that never happened in time
/// </summary>
public interface IPackageIntakeService
{
    Task<string> RequestPaymentAsync(int packageId, string payerEmail, decimal amount);
    Task ConfirmPaymentAsync(int packageId);

    /// <summary>
    /// called by a courier who physically found the package in its box during pickup.
    /// deliberately not something the sender can trigger themselves - a sender claiming
    /// "I put it in" proves nothing, only the courier who opens the box can confirm it
    /// </summary>
    Task ConfirmDepositAsync(int packageId, int courierId);

    /// <summary>
    /// called by a courier who opened the box and found it empty. cancels the order
    /// immediately instead of waiting for the deposit deadline to pass
    /// </summary>
    Task ReportMissingDepositAsync(int packageId, int courierId);

    Task ProcessMissedDepositsAsync();
}
