namespace PackageDeliverySystem.Core;

/// <summary>
/// lets a package's recipient rate the courier and/or the sender of that one delivery,
/// once it has actually been collected
/// </summary>
public interface IRatingService
{
    Task RateCourierAsync(int packageId, int raterUserId, int score);
    Task RateSenderAsync(int packageId, int raterUserId, int score);
}
