namespace PackageDeliverySystem.Core;

public class User : AbstractPerson
{
    public List<Package>? SentPackages { get; set; } = new List<Package>();
    public List<Package>? ReceivedPackages { get; set; } = new List<Package>();
}
