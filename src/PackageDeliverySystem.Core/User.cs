namespace PackageDeliverySystem.Core;

public class User : IPeople
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
