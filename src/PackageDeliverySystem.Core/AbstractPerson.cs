namespace PackageDeliverySystem.Core;

public abstract class AbstractPerson
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// 1-5 rating left by a recipient after collecting a delivery, for a courier or a sender.
    /// null until first rated. overwritten by each new rating - no history is kept.
    /// set via IPeopleService.UpdateRankAsync, never directly
    /// </summary>
    public int? Rank { get; set; }
}
