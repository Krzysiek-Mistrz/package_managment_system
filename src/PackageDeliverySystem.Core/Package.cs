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

    public void Accept()
    {
        if (Status != PackageStatus.Ordered)
            throw new InvalidOperationException($"Cannot accept package in status {Status}");
        
        Status = PackageStatus.Accepted;
    }

    public void Prepare()
    {
        if (Status != PackageStatus.Accepted)
            throw new InvalidOperationException($"Cannot prepare package in status {Status}");

        Status = PackageStatus.Preparing;
    }

    public void Ship()
    {
        if (Status != PackageStatus.Preparing)
            throw new InvalidOperationException($"Cannot ship package in status {Status}");

        Status = PackageStatus.Shipped;
    }

    public void Deliver()
    {
        if (Status != PackageStatus.Shipped)
            throw new InvalidOperationException($"Cannot deliver package in status {Status}");

        Status = PackageStatus.Delivered;
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
}
