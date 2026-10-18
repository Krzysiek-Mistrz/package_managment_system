namespace PackageDeliverySystem.Core;

public class Courier : AbstractPerson
{
    // Coordinates for assignment logic later
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public List<Package>? Packages { get; set; } = new List<Package>();
}
