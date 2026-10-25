namespace PackageDeliverySystem.Core;

public class Payment
{
    public int Id { get; set; } = 0;
    public int PackageId { get; set; }
    public Package Package { get; set; } = null!;
    public string PayerEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string? ExternalReference { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; private set; }

    public void Complete()
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot complete payment in status {Status}");
        }

        Status = PaymentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot fail payment in status {Status}");
        }

        Status = PaymentStatus.Failed;
    }
}
