using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PackageDeliverySystem.Infrastructure.Data;
using PackageDeliverySystem.Infrastructure;
using PackageDeliverySystem.Core.Interfaces;
using PackageDeliverySystem.Core;

// setup DI
var services = new ServiceCollection();

// configure SQLite
string connectionString = "Data Source=delivery.db";
services.AddDbContext<DeliveryDbContext>(options =>
    options.UseSqlite(connectionString));

// register repositories
services.AddScoped<IPackageRepository, PackageRepository>();
var serviceProvider = services.BuildServiceProvider();

// create db file automatically:
using (var scope = serviceProvider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();
    db.Database.EnsureCreated();

    Console.WriteLine("--- Package Delivery System Scenario ---");

    var sender = new User { Id = 1, Name = "Alice" };
    var recipient = new User { Id = 2, Name = "Bob" };
    var package = new Package
    {
        Id = 1,
        Sender = sender,
        Recipient = recipient,
        Status = PackageStatus.Ordered
    };

    Console.WriteLine($"Initial Status: {package.Status}");

    try
    {
        package.Accept();
        Console.WriteLine($"After Accept: {package.Status}");

        package.Prepare();
        Console.WriteLine($"After Prepare: {package.Status}");

        package.Ship();
        Console.WriteLine($"After Ship: {package.Status}");

        package.Deliver();
        Console.WriteLine($"After Deliver: {package.Status}");

        Console.WriteLine("Scenario completed successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Scenario failed: {ex.Message}");
    }
}
