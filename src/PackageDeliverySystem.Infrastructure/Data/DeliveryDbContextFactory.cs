using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PackageDeliverySystem.Infrastructure.Data;

/// <summary>
/// lets dotnet-ef create the context without running the cli app
/// </summary>
public class DeliveryDbContextFactory : IDesignTimeDbContextFactory<DeliveryDbContext>
{
    public DeliveryDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DeliveryDbContext>()
            .UseMySQL(DatabaseSettings.GetConnectionString())
            .Options;

        return new DeliveryDbContext(options);
    }
}
