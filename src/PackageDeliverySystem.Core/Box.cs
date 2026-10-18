namespace PackageDeliverySystem.Core;

public class Box
{
    public int Id { get; set; } = 0;
    public int ParcelLockerId { get; set; }
    public ParcelLocker? ParcelLocker { get; set; } = null!;
    public List<Package>? Packages { get; set; } = new List<Package>();
    public BoxSize Size { get; set; }
}
