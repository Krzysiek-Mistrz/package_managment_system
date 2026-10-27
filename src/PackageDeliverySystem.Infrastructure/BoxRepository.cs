using Microsoft.EntityFrameworkCore;
using PackageDeliverySystem.Core;
using PackageDeliverySystem.Infrastructure.Data;

namespace PackageDeliverySystem.Infrastructure;

public class BoxRepository : IBoxRepository
{
    private readonly DeliveryDbContext _context;

    public BoxRepository(DeliveryDbContext context)
    {
        _context = context;
    }

    public async Task<Box?> GetByIdAsync(int boxId)
        => await _context.Boxes.FirstOrDefaultAsync(b => b.Id == boxId);

    // a box stays reserved by a package (as either its origin or its destination) for that
    // package's whole active life - from the moment it's ordered until it's either collected
    // or falls into one of these terminal states, none of which leave anything in the box
    private static readonly PackageStatus[] TerminalStatuses =
    {
        PackageStatus.Collected,
        PackageStatus.Cancelled,
        PackageStatus.Rejected,
        PackageStatus.Expired,
        PackageStatus.ReturnedToSender
    };

    public async Task<Box?> FindFreeBoxAsync(int parcelLockerId, BoxSize size, int? excludeBoxId = null)
    {
        var candidates = await _context.Boxes
            .Where(b => b.ParcelLockerId == parcelLockerId && b.Size == size && b.Id != excludeBoxId)
            .ToListAsync();

        foreach (var box in candidates)
        {
            var occupied = await _context.Packages.AnyAsync(p =>
                (p.BoxId == box.Id || p.DestinationBoxId == box.Id) &&
                !TerminalStatuses.Contains(p.Status));

            if (!occupied)
                return box;
        }

        return null;
    }
}
