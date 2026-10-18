namespace PackageDeliverySystem.Core;

public class ParcelLocker
{
    public int Id { get; set; } = 0;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    private readonly List<Box> _boxes = new List<Box>();
    public IReadOnlyList<Box> Boxes => _boxes.AsReadOnly();
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;

    public Box CreateBox(BoxSize size)
    {
        var box = new Box
        {
            Size = size,
            ParcelLocker = this
        };
        _boxes.Add(box);
        return box;
    }

    public void RemoveBox(int boxId)
    {
        var box = _boxes.FirstOrDefault(b => b.Id == boxId);
        if (box != null)
        {
            _boxes.Remove(box);
        }
    }
}
