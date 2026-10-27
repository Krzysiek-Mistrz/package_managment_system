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
services.AddScoped<IRatingService, RatingService>();
services.AddScoped<ISupportContactService, SupportContactService>();
services.AddScoped<IPackageOrderService, PackageOrderService>();

var serviceProvider = services.BuildServiceProvider();

using (var scope = serviceProvider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();
    db.Database.Migrate();
    SeedDemoData(db);

    var acceptanceService = scope.ServiceProvider.GetRequiredService<IPackageAcceptanceService>();
    var fulfillmentService = scope.ServiceProvider.GetRequiredService<IPackageFulfillmentService>();
    var intakeService = scope.ServiceProvider.GetRequiredService<IPackageIntakeService>();
    var ratingService = scope.ServiceProvider.GetRequiredService<IRatingService>();
    var supportContactService = scope.ServiceProvider.GetRequiredService<ISupportContactService>();
    var orderService = scope.ServiceProvider.GetRequiredService<IPackageOrderService>();
    var packageRepo = scope.ServiceProvider.GetRequiredService<IPackageRepository>();

    Console.WriteLine("--- Package Delivery System Scenario ---");

    var sender = db.Users.OfType<User>().OrderBy(u => u.Id).First();
    var recipient = db.Users.OfType<User>().OrderBy(u => u.Id).Skip(1).First();
    var operatorUser = db.Users.OfType<Operator>().First();
    var courier = db.Users.OfType<Courier>().First();
    var originLocker = db.ParcelLockers.OrderBy(l => l.Id).First();
    var destinationLocker = db.ParcelLockers.OrderBy(l => l.Id).Skip(1).First();

    try
    {
        var package = await orderService.CreatePackageAsync(
            sender.Id, recipient.Id, originLocker.Id, destinationLocker.Id, BoxSize.Medium);
        var boxId = package.BoxId!.Value;
        Console.WriteLine(
            $"Package {package.Id} ordered: box {boxId} at origin, box {package.DestinationBoxId} at destination.");

        var paymentReference = await intakeService.RequestPaymentAsync(package.Id, sender.Email, 14.99m);
        Console.WriteLine($"Payment requested, reference: {paymentReference}");

        await intakeService.ConfirmPaymentAsync(package.Id);
        Console.WriteLine("Payment confirmed.");

        await intakeService.ConfirmDepositAsync(package.Id, courier.Id);
        Console.WriteLine("Deposit confirmed: courier found the package in its box.");

        await acceptanceService.AcceptPackageAsync(package.Id, operatorUser.Id, boxId);
        await PrintStatusAsync(packageRepo, package.Id);

        await fulfillmentService.AssignCourierAsync(package.Id, courier.Id);
        await fulfillmentService.ShipPackageAsync(package.Id);
        await PrintStatusAsync(packageRepo, package.Id);

        await fulfillmentService.DeliverPackageAsync(package.Id);
        var deliveredPackage = await packageRepo.GetByIdAsync(package.Id);
        Console.WriteLine($"Package {deliveredPackage!.Id} status: {deliveredPackage.Status}, pickup code: {deliveredPackage.PickupCode}");

        await fulfillmentService.CollectPackageAsync(package.Id, recipient.Id, deliveredPackage.PickupCode!);
        await PrintStatusAsync(packageRepo, package.Id);

        await ratingService.RateCourierAsync(package.Id, recipient.Id, 5);
        await ratingService.RateSenderAsync(package.Id, recipient.Id, 4);

        // same DbContext instance as the services above (same DI scope), so courier.Rank
        // is already updated in memory - no re-query needed to see the effect
        Console.WriteLine($"Courier {courier.Name} rank is now: {courier.Rank}");
        Console.WriteLine($"Sender {sender.Name} rank is now: {sender.Rank}");
        Console.WriteLine($"Operator {operatorUser.Name} rank is still: {operatorUser.Rank?.ToString() ?? "null"}");

        await supportContactService.ContactSupportAsync(
            recipient.Id, "Package arrived damaged", "The box was crushed on one side, please advise.");
        Console.WriteLine($"Support contacted by {recipient.Name} - check Mailpit for the message.");

        // second package, taking the reject branch instead of the happy path
        var rejectedPackage = await orderService.CreatePackageAsync(
            sender.Id, recipient.Id, originLocker.Id, destinationLocker.Id, BoxSize.Small);
        await intakeService.RequestPaymentAsync(rejectedPackage.Id, sender.Email, 9.99m);
        await intakeService.ConfirmPaymentAsync(rejectedPackage.Id);
        await intakeService.ConfirmDepositAsync(rejectedPackage.Id, courier.Id);

        await acceptanceService.RejectPackageAsync(rejectedPackage.Id, operatorUser.Id);
        await PrintStatusAsync(packageRepo, rejectedPackage.Id);
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

    var originLocker = new ParcelLocker
    {
        Latitude = 50.0647,
        Longitude = 19.9450,
        Country = "Poland",
        City = "Krakow",
        PostalCode = "30-059",
        Street = "Mickiewicza",
        Number = "30"
    };
    originLocker.CreateBox(BoxSize.Small);
    originLocker.CreateBox(BoxSize.Medium);
    originLocker.CreateBox(BoxSize.Large);

    var destinationLocker = new ParcelLocker
    {
        Latitude = 52.2297,
        Longitude = 21.0122,
        Country = "Poland",
        City = "Warszawa",
        PostalCode = "00-001",
        Street = "Marszalkowska",
        Number = "10"
    };
    destinationLocker.CreateBox(BoxSize.Small);
    destinationLocker.CreateBox(BoxSize.Medium);
    destinationLocker.CreateBox(BoxSize.Large);

    db.ParcelLockers.AddRange(originLocker, destinationLocker);
    db.Users.AddRange(
        new User { Name = "Alice", Surname = "Nowak", Email = "alice@example.com", PhoneNumber = "500100200" },
        new User { Name = "Bob", Surname = "Kowalski", Email = "bob@example.com", PhoneNumber = "500100201" },
        new Operator { Name = "Charlie", Surname = "Wisniewski", Email = "charlie@example.com", PhoneNumber = "500100202" },
        new Courier { Name = "Dave", Surname = "Zielinski", Email = "dave@example.com", PhoneNumber = "500100203", Latitude = 50.06, Longitude = 19.94 });
    db.SaveChanges();
}
