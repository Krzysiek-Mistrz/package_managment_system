using Microsoft.EntityFrameworkCore;
using PackageDeliverySystem.Core;
using PackageDeliverySystem.Infrastructure.Data;
using Xunit;
using Moq;

namespace PackageDeliverySystem.Tests;

public class PackageTests
{
    /// <summary>
    /// Accept() now requires a confirmed deposit and a completed payment - this satisfies
    /// both preconditions directly on the in-memory object, without needing a database
    /// </summary>
    private static void MakeReadyForAcceptance(Package package)
    {
        package.ConfirmDeposit();
        package.Payment = new Payment { PackageId = package.Id, PayerEmail = "payer@example.com", Amount = 14.99m };
        package.Payment.Complete();
    }

    private readonly Mock<IPackageRepository> _packageRepoMock;
    private readonly Mock<IBoxRepository> _boxRepoMock;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly Mock<IPeopleService> _peopleMock;
    private readonly PackageAcceptanceService _service;

    public PackageTests()
    {
        _packageRepoMock = new Mock<IPackageRepository>();
        _boxRepoMock = new Mock<IBoxRepository>();
        _notificationMock = new Mock<INotificationService>();
        _peopleMock = new Mock<IPeopleService>();

        _service = new PackageAcceptanceService(
            _packageRepoMock.Object,
            _boxRepoMock.Object,
            _notificationMock.Object,
            _peopleMock.Object);
    }

    [Fact]
    public void Package_Should_Follow_Valid_Workflow()
    {
        var package = new Package
        {
            Sender = new User { Id = 1, Name = "Sender" },
            Recipient = new User { Id = 2, Name = "Recipient" }
        };

        Assert.Equal(PackageStatus.Ordered, package.Status);

        MakeReadyForAcceptance(package);
        package.Accept();
        Assert.Equal(PackageStatus.Accepted, package.Status);

        package.Prepare();
        Assert.Equal(PackageStatus.Preparing, package.Status);

        package.Ship();
        Assert.Equal(PackageStatus.Shipped, package.Status);

        package.Deliver();
        Assert.Equal(PackageStatus.Delivered, package.Status);
    }

    [Fact]
    public void Package_Should_Throw_Exception_On_Invalid_Transition()
    {
        var package = new Package
        {
            Sender = new User { Id = 1, Name = "Sender" },
            Recipient = new User { Id = 2, Name = "Recipient" }
        };

        Assert.Throws<InvalidOperationException>(() => package.Ship());
    }

    [Theory]
    [InlineData(PackageStatus.Ordered)]
    [InlineData(PackageStatus.Accepted)]
    public void Package_Should_Allow_Cancellation_From_Valid_States(PackageStatus initialStatus)
    {
        var package = new Package
        {
            Sender = new User { Id = 1, Name = "Sender" },
            Recipient = new User { Id = 2, Name = "Recipient" }
        };

        if (initialStatus == PackageStatus.Accepted)
        {
            MakeReadyForAcceptance(package);
            package.Accept();
        }

        package.Cancel();

        Assert.Equal(PackageStatus.Cancelled, package.Status);
    }

    [Fact]
    public void Package_Should_Allow_Rejection_From_Ordered_Or_Accepted()
    {
        var package = new Package
        {
            Sender = new User { Id = 1, Name = "Sender" },
            Recipient = new User { Id = 2, Name = "Recipient" }
        };

        package.Reject();

        Assert.Equal(PackageStatus.Rejected, package.Status);
    }

    [Fact]
    public async Task AcceptPackageAsync_ValidRequest_ShouldUpdateStatusAndNotify()
    {
        var package = new Package { Id = 1, BoxId = 10 };
        MakeReadyForAcceptance(package);
        var operatorUser = new Operator { Id = 5, Name = "John Doe" };

        // when code asks 4 GetByIdAsync don't go to the database. Just return package obj
        _packageRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(package);
        _boxRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Box { Id = 10, Size = package.Size });
        _peopleMock.Setup(p => p.GetOperatorAsync(5)).ReturnsAsync(operatorUser);

        await _service.AcceptPackageAsync(1, 5, 10);

        Assert.Equal(PackageStatus.Accepted, package.Status);
        Assert.Equal(operatorUser, package.Operator);
        // did the service actually call the UpdateAsync method just once?
        _packageRepoMock.Verify(r => r.UpdateAsync(package), Times.Once);
        _notificationMock.Verify(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AcceptPackageAsync_WrongBoxId_ShouldThrowArgumentException()
    {
        var package = new Package { Id = 1, BoxId = 10 };
        _packageRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(package);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.AcceptPackageAsync(1, 5, 99));
    }

    [Fact]
    public async Task AcceptPackageAsync_PackageTooLargeForBox_ShouldThrowInvalidOperationException()
    {
        var package = new Package { Id = 1, BoxId = 10, Size = BoxSize.Large };
        MakeReadyForAcceptance(package);
        var operatorUser = new Operator { Id = 5, Name = "John Doe" };

        _packageRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(package);
        _boxRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Box { Id = 10, Size = BoxSize.Small });
        _peopleMock.Setup(p => p.GetOperatorAsync(5)).ReturnsAsync(operatorUser);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AcceptPackageAsync(1, 5, 10));
    }

    [Fact]
    public async Task ProcessExpiredPackagesAsync_ShouldExpirePackages()
    {
        var expiredPackage = new Package { Id = 1 };
        MakeReadyForAcceptance(expiredPackage);
        expiredPackage.Accept();
        
        typeof(Package).GetProperty(nameof(Package.ExpiredAt))?.SetValue(expiredPackage, DateTime.UtcNow.AddHours(-1));

        _packageRepoMock.Setup(r => r.GetPackagesRequiringExpirationAsync())
            .ReturnsAsync(new List<Package> { expiredPackage });

        await _service.ProcessExpiredPackagesAsync();

        Assert.Equal(PackageStatus.Expired, expiredPackage.Status);
        _packageRepoMock.Verify(r => r.UpdateAsync(expiredPackage), Times.Once);
        _notificationMock.Verify(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
    }

    /// <summary>
    /// prototype -> integration test against a real mysql, proving that 2 operators racing
    /// for the same box cannot both succeed. requires "docker compose up -d" and
    /// PDS_CONNECTION_STRING to be set before running dotnet test
    /// </summary>
    [Fact]
    public async Task TwoOperators_RacingForTheSameBox_OnlyOneSucceeds()
    {
        DeliveryDbContext CreateContext() => new(
            new DbContextOptionsBuilder<DeliveryDbContext>()
                .UseMySQL(DatabaseSettings.GetConnectionString())
                .Options);

        int boxId;
        await using (var setupContext = CreateContext())
        {
            var locker = new ParcelLocker { City = "Test", Street = "Test", Number = "1", PostalCode = "00-000", Country = "Poland" };
            var box = locker.CreateBox(BoxSize.Medium);
            setupContext.ParcelLockers.Add(locker);
            await setupContext.SaveChangesAsync();
            boxId = box.Id;
        }

        await using var contextA = CreateContext();
        await using var contextB = CreateContext();
        var boxForOperatorA = await contextA.Boxes.FirstAsync(b => b.Id == boxId);
        var boxForOperatorB = await contextB.Boxes.FirstAsync(b => b.Id == boxId);

        boxForOperatorA.Version++;
        boxForOperatorB.Version++;
        var saveA = contextA.SaveChangesAsync();
        var saveB = contextB.SaveChangesAsync();

        var succeededCount = 0;
        var concurrencyExceptionCount = 0;

        foreach (var save in new[] { saveA, saveB })
        {
            try
            {
                await save;
                succeededCount++;
            }
            catch (DbUpdateConcurrencyException)
            {
                concurrencyExceptionCount++;
            }
        }

        Assert.Equal(1, succeededCount);
        Assert.Equal(1, concurrencyExceptionCount);
    }
}
