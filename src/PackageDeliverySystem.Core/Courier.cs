namespace PackageDeliverySystem.Core;

public class Courier : IPeople
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    // Coordinates for assignment logic later
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
