namespace PackageDeliverySystem.Core;

public class Operator : AbstractPerson
{
    public List<Package>? Packages { get; set; } = new List<Package>();
}
