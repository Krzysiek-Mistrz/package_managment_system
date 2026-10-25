namespace PackageDeliverySystem.Core;

/// <summary>
/// extension point: an implementer plugs in their real payment provider here
/// (stripe, przelewy24, payu, ...). the framework only ever needs an e-mail to start
/// a payment and a reference to check its outcome later - no account or login on our side.
/// </summary>
public interface IPaymentGateway
{
    Task<string> InitiatePaymentAsync(string payerEmail, decimal amount);
    Task<PaymentStatus> GetPaymentStatusAsync(string externalReference);
}
