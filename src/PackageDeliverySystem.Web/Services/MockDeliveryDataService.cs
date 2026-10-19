using PackageDeliverySystem.Core;
using System.Collections.Generic;

namespace PackageDeliverySystem.Web.Services;

/// <summary>
/// in memory mock data 4 blazor mockup screens no db involved
/// </summary>
public class MockDeliveryDataService : IDeliveryDataService
{
    private readonly List<Package> _packages = new();
    private readonly List<ParcelLocker> _parcelLockers = new();
    private int _nextPackageId = 1;

    public MockDeliveryDataService()
    {
        var lockerA = new ParcelLocker { Id = 1, City = "Krakow", Street = "Mickiewicza", Number = "30", PostalCode = "30-059", Country = "Poland" };
        var lockerB = new ParcelLocker { Id = 2, City = "Warszawa", Street = "Marszalkowska", Number = "10", PostalCode = "00-001", Country = "Poland" };

        var boxA1 = lockerA.CreateBox(BoxSize.Small);
        var boxA2 = lockerA.CreateBox(BoxSize.Medium);
        var boxB1 = lockerB.CreateBox(BoxSize.Large);
        boxA1.Id = 1;
        boxA2.Id = 2;
        boxB1.Id = 3;

        _parcelLockers.Add(lockerA);
        _parcelLockers.Add(lockerB);

        var alice = new User { Id = 1, Name = "Alice", Surname = "Nowak", Email = "alice@example.com", PhoneNumber = "500100200" };
        var bob = new User { Id = 2, Name = "Bob", Surname = "Kowalski", Email = "bob@example.com", PhoneNumber = "500100201" };
        var ewa = new User { Id = 3, Name = "Ewa", Surname = "Lis", Email = "ewa@example.com", PhoneNumber = "500100204" };

        var operatorUser = new Operator { Id = 1, Name = "Charlie", Surname = "Wisniewski", Email = "charlie@example.com", PhoneNumber = "500100202" };
        var courier = new Courier { Id = 1, Name = "Dave", Surname = "Zielinski", Email = "dave@example.com", PhoneNumber = "500100203", Latitude = 50.06, Longitude = 19.94 };

        _packages.Add(CreatePackage(alice, bob, boxA1, null, PackageStatus.Ordered));
        _packages.Add(CreatePackage(bob, ewa, boxA2, boxB1, PackageStatus.Accepted, operatorUser));
        _packages.Add(CreatePackage(ewa, alice, boxB1, boxA1, PackageStatus.Shipped, operatorUser, courier));
        _packages.Add(CreatePackage(alice, ewa, boxA1, boxA2, PackageStatus.Delivered, operatorUser, courier));
        _packages.Add(CreatePackage(bob, alice, boxA2, null, PackageStatus.Ordered));
    }

    /// <summary>
    /// builds mock package and drives it up to target status. box objects (not just ids) are passed in
    /// so the Box/DestinationBox navigations are populated too, since this mock has no EF behind it to resolve them
    /// </summary>
    private Package CreatePackage(User sender, User recipent, Box box, Box? destinationBox, PackageStatus targetStatus, Operator? operatorUser = null, Courier? courier = null)
    {
        var package = new Package
        {
            Id = _nextPackageId++,
            Sender = sender,
            SenderId = sender.Id,
            Recipient = recipent,
            RecipientId = recipent.Id,
            Box = box,
            BoxId = box.Id,
            DestinationBox = destinationBox,
            DestinationBoxId = destinationBox?.Id,
        };

        if (targetStatus == PackageStatus.Ordered)
            return package;

        package.Accept();

        if (operatorUser != null)
        {
            package.AssignOperator(operatorUser);
        }
        if (courier != null)
        {
            package.AssignCourier(courier);
        }

        if (targetStatus == PackageStatus.Accepted)
            return package;

        package.Prepare();
        package.Ship();

        if (targetStatus == PackageStatus.Shipped)
            return package;

        package.Deliver();
        return package;
    }

    public Task<IEnumerable<Package>> GetAllPackages()
        => Task.FromResult(_packages.AsEnumerable());

    public Task<IEnumerable<Package>> GetOrderedPackages()
        => Task.FromResult(_packages.Where(p => p.Status == PackageStatus.Ordered));

    public Task<Package?> GetPackage(int id)
        => Task.FromResult(_packages.FirstOrDefault(p => p.Id == id));

    public Task<IEnumerable<Package>> GetCourierPackages(int courierId)
        => Task.FromResult(_packages.Where(p => p.CourierId == courierId));

    public Task<IEnumerable<ParcelLocker>> GetParcelLockers()
        => Task.FromResult(_parcelLockers.AsEnumerable());

    public Task AddPackage(Package package)
    {
        package.Id = _nextPackageId++;
        _packages.Add(package);
        return Task.CompletedTask;
    }
}