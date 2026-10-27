using Microsoft.EntityFrameworkCore;
using PackageDeliverySystem.Core;
using PackageDeliverySystem.Infrastructure.Data;

namespace PackageDeliverySystem.Infrastructure;

/// <summary>
/// works for both users and couriers (and operators, if ever needed) since it looks the
/// person up as a plain AbstractPerson - no need to know which subtype is contacting support
/// </summary>
public class SupportContactService : ISupportContactService
{
    private readonly DeliveryDbContext _context;
    private readonly INotificationService _notificationService;

    public SupportContactService(DeliveryDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task ContactSupportAsync(int personId, string subject, string message)
    {
        var person = await _context.Users.FirstOrDefaultAsync(u => u.Id == personId)
            ?? throw new ArgumentException("Person not found");

        await _notificationService.NotifyAsync(
            "support",
            $"[{person.Name} {person.Surname} <{person.Email}>] {subject}: {message}");
    }
}
