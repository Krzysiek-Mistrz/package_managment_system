using PackageDeliverySystem.Core;

namespace PackageDeliverySystem.Infrastructure;

/// <summary>
/// lets a package's recipient rate the courier and/or the sender of that one delivery,
/// once it has been collected. just overwrites AbstractPerson.Rank directly - no audit trail
/// </summary>
public class RatingService : IRatingService
{
    private readonly IPackageRepository _packageRepository;
    private readonly IPeopleService _peopleService;

    public RatingService(IPackageRepository packageRepository, IPeopleService peopleService)
    {
        _packageRepository = packageRepository;
        _peopleService = peopleService;
    }

    public Task RateCourierAsync(int packageId, int raterUserId, int score)
        => RatePersonAsync(packageId, raterUserId, score, p => p.CourierId, "courier");

    public Task RateSenderAsync(int packageId, int raterUserId, int score)
        => RatePersonAsync(packageId, raterUserId, score, p => p.SenderId, "sender");

    /// <summary>
    /// shared guard logic for both RateCourierAsync and RateSenderAsync (DRY): score must be
    /// 1-5, the package must actually be collected, only its real recipient may rate it, and
    /// the target role must exist on this package
    /// </summary>
    private async Task RatePersonAsync(
        int packageId, int raterUserId, int score, Func<Package, int?> selectRatedPersonId, string roleLabel)
    {
        if (score is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(score), "Score must be between 1 and 5");

        var package = await _packageRepository.GetByIdAsync(packageId)
            ?? throw new ArgumentException("Package not found");

        if (package.Status != PackageStatus.Collected)
            throw new InvalidOperationException("Can only rate a delivery after it has been collected");
        if (package.RecipientId != raterUserId)
            throw new InvalidOperationException("Only this package's recipient can rate it");

        var ratedPersonId = selectRatedPersonId(package)
            ?? throw new InvalidOperationException($"This package has no {roleLabel} to rate");

        await _peopleService.UpdateRankAsync(ratedPersonId, score);
    }
}
