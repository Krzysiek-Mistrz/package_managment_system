namespace PackageDeliverySystem.Core;

public class User : IPeople
{
    public int Id { get; set; } = 0;
    public string Name { get; set; } = string.Empty;
}
