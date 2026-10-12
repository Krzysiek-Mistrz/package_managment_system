using PackageDeliverySystem.Core;
using PackageDeliverySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PackageDeliverySystem.Infrastructure;

public class PeopleService : IPeopleService
{
    private readonly DeliveryDbContext _context;

    public PeopleService(DeliveryDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// retrieves a user by their unique identifier
    /// </summary>
    public async Task<User?> GetUserAsync(int userId) 
        => await _context.Users.FindAsync(userId);

    /// <summary>
    /// retrieves an operator by their unique identifier
    /// </summary>
    public async Task<Operator?> GetOperatorAsync(int operatorId) 
        => await _context.Set<Operator>().FindAsync(operatorId);

    /// <summary>
    /// retrieves a courier by their unique identifier
    /// </summary>
    public async Task<Courier?> GetCourierAsync(int courierId) 
        => await _context.Set<Courier>().FindAsync(courierId);
}