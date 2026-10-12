namespace PackageDeliverySystem.Core;

public class ParcelLocker
{
    public string Id { get; set; } = string.Empty;
    public string Location { get; set; } = null!;
    private readonly List<Box> _boxes = new List<Box>();
    public IReadOnlyList<Box> Boxes => _boxes.AsReadOnly();

    public void AddBox(Box box)
    {
        _boxes.Add(box);
    }

    public void RemoveBox(string boxId)
    {
        var box = _boxes.FirstOrDefault(b => b.Id == boxId);
        if (box != null)
        {
            _boxes.Remove(box);
        }
    }
}
