namespace PackageDeliverySystem.Core;

public class Package
{
    public int Id { get; set; } = 0;
    public int BoxId { get; set; }
    public PackageStatus Status { get; private set; } = PackageStatus.Ordered;
    public User Sender { get; set; } = null!;
    public User Recipient { get; set; } = null!;
    public Courier? Courier { get; set; }
    public Operator? Operator { get; set; }
    public Box? Box { get; set; } = null!;

    public DateTime? AcceptedAt { get; private set; }
    public DateTime? PreparedAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CollectedAt { get; private set; }
    public DateTime? ExpiredAt { get; private set; }

    public void Accept()
    {
        if (Status != PackageStatus.Ordered)
            throw new InvalidOperationException($"Cannot accept package in status {Status}");
        
        Status = PackageStatus.Accepted;
        AcceptedAt = DateTime.UtcNow;
        ExpiredAt = AcceptedAt.Value.AddHours(48);
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

    public void Deliver()
    {
        if (Status != PackageStatus.Shipped)
            throw new InvalidOperationException($"Cannot deliver package in status {Status}");

        Status = PackageStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
    }

    public void Collect()
    {
        if (Status != PackageStatus.Delivered)
            throw new InvalidOperationException($"Cannot collect package in status {Status}");

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
            throw new InvalidOperationException($"Cannot assign operator to package in status {Status}");

        Courier = courier;
    }
}
