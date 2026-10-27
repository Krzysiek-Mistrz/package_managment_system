namespace PackageDeliverySystem.Core;

public class Package
{
    // the whole flow is "order remotely, then go drop it off / pick it up" (not "stand at
    // the locker while ordering"), so every stage gets the same 48h window - deposit,
    // acceptance-to-shipping and shipping-to-collection alike. one rule, easy to explain.
    private static readonly TimeSpan DepositWindow = TimeSpan.FromHours(48);

    public int Id { get; set; } = 0;
    public BoxSize Size { get; set; }
    public int? BoxId { get; set; }
    public int? DestinationBoxId { get; set; }
    public PackageStatus Status { get; private set; } = PackageStatus.Ordered;
    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;
    public int RecipientId { get; set; }
    public User Recipient { get; set; } = null!;
    public int? CourierId { get; set; }
    public Courier? Courier { get; set; }
    public int? OperatorId { get; set; }
    public Operator? Operator { get; set; }
    public Box? Box { get; set; } = null!;
    public Box? DestinationBox { get; set; }
    public Payment? Payment { get; set; }

    public DateTime DepositDeadline { get; set; } = DateTime.UtcNow.Add(DepositWindow);
    public DateTime? DepositConfirmedAt { get; private set; }

    public DateTime? AcceptedAt { get; private set; }
    public DateTime? PreparedAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CollectedAt { get; private set; }
    public DateTime? ExpiredAt { get; private set; }
    public string? PickupCode { get; private set; }

    public void Accept()
    {
        if (Status != PackageStatus.Ordered)
            throw new InvalidOperationException($"Cannot accept package in status {Status}");
        if (DepositConfirmedAt is null)
            throw new InvalidOperationException("Cannot accept a package that was never deposited into its box");
        if (Payment is null || Payment.Status != PaymentStatus.Completed)
            throw new InvalidOperationException("Cannot accept a package that has not been paid for");

        Status = PackageStatus.Accepted;
        AcceptedAt = DateTime.UtcNow;
        ExpiredAt = AcceptedAt.Value.AddHours(48);
    }

    /// <summary>
    /// called once the sender confirms (e.g. by closing the locker door) that the package
    /// physically went into its box. must happen within the deposit window, otherwise the
    /// reservation is meant to be released by ProcessMissedDepositsAsync instead
    /// </summary>
    public void ConfirmDeposit()
    {
        if (Status != PackageStatus.Ordered)
            throw new InvalidOperationException($"Cannot confirm deposit for package in status {Status}");
        if (DateTime.UtcNow > DepositDeadline)
            throw new InvalidOperationException("Deposit window has expired; the order must be re-created");

        DepositConfirmedAt = DateTime.UtcNow;
    }

    public void Prepare()
    {
        if (Status != PackageStatus.Accepted)
            throw new InvalidOperationException($"Cannot prepare package in status {Status}");

        Status = PackageStatus.Preparing;
        PreparedAt = DateTime.UtcNow;
    }

    public void Ship()
    {
        if (Status != PackageStatus.Preparing)
            throw new InvalidOperationException($"Cannot ship package in status {Status}");

        Status = PackageStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        ExpiredAt = ShippedAt.Value.AddHours(48);
    }

    /// <summary>
    /// generates the pickup code here, not earlier - a code that existed since Ordered would
    /// sit around unused (and guessable) for up to 48h before anyone needs it
    /// </summary>
    public void Deliver()
    {
        if (Status != PackageStatus.Shipped)
            throw new InvalidOperationException($"Cannot deliver package in status {Status}");

        Status = PackageStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        PickupCode = Random.Shared.Next(100_000, 999_999).ToString();
    }

    public void Collect(string code)
    {
        if (Status != PackageStatus.Delivered)
            throw new InvalidOperationException($"Cannot collect package in status {Status}");
        if (PickupCode is null || code != PickupCode)
            throw new InvalidOperationException("Invalid pickup code");

        Status = PackageStatus.Collected;
        CollectedAt = DateTime.UtcNow;
    }

    public void MarkAsReturned()
    {
        if (Status == PackageStatus.Delivered || Status == PackageStatus.Cancelled || Status == PackageStatus.Rejected)
             throw new InvalidOperationException($"Cannot return package in status {Status}");

        Status = PackageStatus.ReturnedToSender;
    }

    public void Reject()
    {
        if (Status != PackageStatus.Ordered && Status != PackageStatus.Accepted)
            throw new InvalidOperationException($"Cannot reject package in status {Status}");

        Status = PackageStatus.Rejected;
    }

    public void Cancel()
    {
        if (Status != PackageStatus.Ordered && Status != PackageStatus.Accepted)
            throw new InvalidOperationException($"Cannot cancel package in status {Status}");

        Status = PackageStatus.Cancelled;
    }

    public void Expire()
        {
            if (Status == PackageStatus.Delivered && ExpiredAt - DateTime.UtcNow > TimeSpan.FromHours(48))
                throw new InvalidOperationException($"Cannot expire package in status {Status}");
            
            Status = PackageStatus.Expired;
        }

    public void AssignOperator(Operator operatorUser)
    {
        if (Status != PackageStatus.Accepted)
            throw new InvalidOperationException($"Cannot assign operator to package in status {Status}");

        Operator = operatorUser;
    }

    public void AssignCourier(Courier courier)
    {
        if (Status != PackageStatus.Accepted)
            throw new InvalidOperationException($"Cannot assign courier to package in status {Status}");

        Courier = courier;
    }
}
