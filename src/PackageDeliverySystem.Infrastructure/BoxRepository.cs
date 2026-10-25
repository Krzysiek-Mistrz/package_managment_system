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
}
