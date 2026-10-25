using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PackageDeliverySystem.Infrastructure.Data;
using PackageDeliverySystem.Infrastructure;
using PackageDeliverySystem.Core;

var services = new ServiceCollection();

services.AddDbContext<DeliveryDbContext>(options =>
    options.UseMySQL(DatabaseSettings.GetConnectionString()));

services.AddScoped<IPackageRepository, PackageRepository>();
services.AddScoped<IBoxRepository, BoxRepository>();
services.AddScoped<IPackageAcceptanceService, PackageAcceptanceService>();
services.AddScoped<IPackageFulfillmentService, PackageFulfillmentService>();
services.AddScoped<IPackageIntakeService, PackageIntakeService>();
services.AddScoped<IPaymentGateway, FakePaymentGateway>();
services.AddScoped<IPeopleService, PeopleService>();
services.AddScoped<INotificationService, NotificationService>();

var serviceProvider = services.BuildServiceProvider();

using (var scope = serviceProvider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();
    db.Database.Migrate();
    SeedDemoData(db);

    var acceptanceService = scope.ServiceProvider.GetRequiredService<IPackageAcceptanceService>();
    var fulfillmentService = scope.ServiceProvider.GetRequiredService<IPackageFulfillmentService>();
    var intakeService = scope.ServiceProvider.GetRequiredService<IPackageIntakeService>();
    var packageRepo = scope.ServiceProvider.GetRequiredService<IPackageRepository>();

    Console.WriteLine("--- Package Delivery System Scenario ---");

    var sender = db.Users.OfType<User>().OrderBy(u => u.Id).First();
    var recipient = db.Users.OfType<User>().OrderBy(u => u.Id).Skip(1).First();
    var operatorUser = db.Users.OfType<Operator>().First();
    var courier = db.Users.OfType<Courier>().First();
    var box = db.Boxes.First();

    var package = new Package
    {
        Sender = sender,
        Recipient = recipient,
        BoxId = box.Id,
        Size = BoxSize.Medium
    };
    await packageRepo.AddAsync(package);

    try
    {
        var paymentReference = await intakeService.RequestPaymentAsync(package.Id, sender.Email, 14.99m);
        Console.WriteLine($"Payment requested, reference: {paymentReference}");

        await intakeService.ConfirmPaymentAsync(package.Id);
        Console.WriteLine("Payment confirmed.");

        await intakeService.ConfirmDepositAsync(package.Id, courier.Id);
        Console.WriteLine("Deposit confirmed: courier found the package in its box.");

        await acceptanceService.AcceptPackageAsync(package.Id, operatorUser.Id, box.Id);
        await PrintStatusAsync(packageRepo, package.Id);

        await fulfillmentService.AssignCourierAsync(package.Id, courier.Id);
        await fulfillmentService.ShipPackageAsync(package.Id);
        await PrintStatusAsync(packageRepo, package.Id);

        await fulfillmentService.DeliverPackageAsync(package.Id);
        await PrintStatusAsync(packageRepo, package.Id);

        await fulfillmentService.CollectPackageAsync(package.Id);
        await PrintStatusAsync(packageRepo, package.Id);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Scenario failed: {ex.Message}");
    }
}

/// <summary>
/// re-reads a package from the repository and prints its current status, so each step
/// of the scenario shows the effect of the service call that just ran
/// </summary>
static async Task PrintStatusAsync(IPackageRepository packageRepo, int packageId)
{
    var package = await packageRepo.GetByIdAsync(packageId);
    Console.WriteLine($"Package {package!.Id} status: {package.Status}");
}

/// <summary>
/// fills an empty database with a parcel locker, boxes and people needed by the demo scenario
/// </summary>
static void SeedDemoData(DeliveryDbContext db)
{
    if (db.Users.Any())
    {
        return;
    }

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
