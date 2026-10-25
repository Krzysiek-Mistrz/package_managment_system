namespace PackageDeliverySystem.Core;

public abstract class AbstractPerson
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// average of all Ratings received as a courier or as a sender. null until first rated.
    /// recalculated from Rating rows, not set by hand - see RatingRepository.RecalculateRankAsync
    /// </summary>
    public int? Rank { get; set; }
}
