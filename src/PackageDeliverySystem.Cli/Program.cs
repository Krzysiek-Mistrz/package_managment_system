using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PackageDeliverySystem.Infrastructure.Data;
using PackageDeliverySystem.Infrastructure;
using PackageDeliverySystem.Core;

var services = new ServiceCollection();

services.AddDbContext<DeliveryDbContext>(options =>
    options.UseMySQL(DatabaseSettings.GetConnectionString()));

services.AddScoped<IPackageRepository, PackageRepository>();
services.AddScoped<IPackageAcceptanceService, PackageAcceptanceService>();
services.AddScoped<IPeopleService, PeopleService>();
services.AddScoped<INotificationService, NotificationService>();

var serviceProvider = services.BuildServiceProvider();

using (var scope = serviceProvider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();
    db.Database.Migrate();
    SeedDemoData(db);

    var acceptanceService = scope.ServiceProvider.GetRequiredService<IPackageAcceptanceService>();
    var packageRepo = scope.ServiceProvider.GetRequiredService<IPackageRepository>();

    Console.WriteLine("--- Package Delivery System Scenario ---");

    var sender = db.Users.OfType<User>().OrderBy(u => u.Id).First();
    var recipient = db.Users.OfType<User>().OrderBy(u => u.Id).Skip(1).First();
    var operatorUser = db.Users.OfType<Operator>().First();
    var box = db.Boxes.First();

    var package = new Package
    {
        Sender = sender,
        Recipient = recipient,
        BoxId = box.Id
    };
    await packageRepo.AddAsync(package);

    try
    {
        await acceptanceService.AcceptPackageAsync(package.Id, operatorUser.Id, box.Id);
        var updatedPackage = await packageRepo.GetByIdAsync(package.Id);

        Console.WriteLine($"Package {updatedPackage!.Id} status: {updatedPackage.Status}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Scenario failed: {ex.Message}");
    }
}

/// <summary>
/// fills an empty database with a parcel locker, boxes and people needed by the demo scenario
/// </summary>
static void SeedDemoData(DeliveryDbContext db)
{
    if (db.Users.Any())
        return;

    var locker = new ParcelLocker
    {
        Latitude = 50.0647,
        Longitude = 19.9450,
        Country = "Poland",
        City = "Krakow",
        PostalCode = "30-059",
        Street = "Mickiewicza",
        Number = "30"
    };
    locker.CreateBox(BoxSize.Small);
    locker.CreateBox(BoxSize.Medium);
    locker.CreateBox(BoxSize.Large);

    db.ParcelLockers.Add(locker);
    db.Users.AddRange(
        new User { Name = "Alice", Surname = "Nowak", Email = "alice@example.com", PhoneNumber = "500100200" },
        new User { Name = "Bob", Surname = "Kowalski", Email = "bob@example.com", PhoneNumber = "500100201" },
        new Operator { Name = "Charlie", Surname = "Wisniewski", Email = "charlie@example.com", PhoneNumber = "500100202" },
        new Courier { Name = "Dave", Surname = "Zielinski", Email = "dave@example.com", PhoneNumber = "500100203", Latitude = 50.06, Longitude = 19.94 });
    db.SaveChanges();
}
