using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PackageDeliverySystem.Infrastructure.Data;
using PackageDeliverySystem.Infrastructure;
using PackageDeliverySystem.Core;

// setup DI
var services = new ServiceCollection();

// configure SQLite
string connectionString = "Data Source=delivery.db";
services.AddDbContext<DeliveryDbContext>(options =>
    options.UseSqlite(connectionString));

// register repositories
services.AddScoped<IPackageRepository, PackageRepository>();
services.AddScoped<IPackageAcceptanceService, PackageAcceptanceService>();
services.AddScoped<IPeopleService, PeopleService>();
services.AddScoped<INotificationService, NotificationService>();

var serviceProvider = services.BuildServiceProvider();

// create db file automatically & running scenario
using (var scope = serviceProvider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();
    db.Database.EnsureCreated();

    var acceptanceService = scope.ServiceProvider.GetRequiredService<IPackageAcceptanceService>();
    var packageRepo = scope.ServiceProvider.GetRequiredService<IPackageRepository>();
    var peopleService = scope.ServiceProvider.GetRequiredService<IPeopleService>();
    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

    Console.WriteLine("--- Package Delivery System Scenario ---");

    var sender = new User { Id = 1, Name = "Alice" };
    var recipient = new User { Id = 2, Name = "Bob" };
    var operatorUser = new User { Id = 3, Name = "Charlie" };
    var box = new Box { Id = 1, Size = BoxSize.Medium };
    var package = new Package
    {
        Id = 1,
        Sender = sender,
        Recipient = recipient
    };
    db.Packages.Add(package);
    db.SaveChanges();

    try
    {
        await acceptanceService.AcceptPackageAsync(package.Id, operatorUser.Id, box.Id);
        var updatedPackage = await packageRepo.GetByIdAsync(package.Id);

        Console.WriteLine($"Package {updatedPackage.Id} status: {updatedPackage.Status}");

        // TODO: after adding more tasks 2 acceptance service requiring full 
        // logic implementation add it here...
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Scenario failed: {ex.Message}");
    }
}
