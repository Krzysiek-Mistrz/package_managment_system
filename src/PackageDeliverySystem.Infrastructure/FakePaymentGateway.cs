using PackageDeliverySystem.Core;

namespace PackageDeliverySystem.Infrastructure;

/// <summary>
/// demo stand-in for a real payment provider, so the intake flow can be exercised end to end
/// without wiring up stripe/przelewy24/payu. a real implementer swaps this for a class that
/// actually redirects to their provider and reads back its real status - nothing else in the
/// framework needs to change, since everything only ever talks to IPaymentGateway
/// </summary>
public class FakePaymentGateway : IPaymentGateway
{
    public Task<string> InitiatePaymentAsync(string payerEmail, decimal amount)
        => Task.FromResult(Guid.NewGuid().ToString());

    public Task<PaymentStatus> GetPaymentStatusAsync(string externalReference)
        => Task.FromResult(PaymentStatus.Completed);
}
