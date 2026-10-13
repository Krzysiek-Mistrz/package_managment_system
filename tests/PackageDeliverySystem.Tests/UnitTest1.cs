using PackageDeliverySystem.Core;
using Xunit;
using Moq;

namespace PackageDeliverySystem.Tests;

public class PackageTests
{
    private readonly Mock<IPackageRepository> _packageRepoMock;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly Mock<IPeopleService> _peopleMock;
    private readonly PackageAcceptanceService _service;

    public PackageTests()
    {
        _packageRepoMock = new Mock<IPackageRepository>();
        _notificationMock = new Mock<INotificationService>();
        _peopleMock = new Mock<IPeopleService>();
        
        _service = new PackageAcceptanceService(
            _packageRepoMock.Object, 
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
        var package = new Package { Id = 1, Status = PackageStatus.Ordered, BoxId = 10 };
        var operatorUser = new Operator { Id = 5, Name = "John Doe" };

        // when code asks 4 GetByIdAsync don't go to the database. Just return package obj
        _packageRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(package);   
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
        var package = new Package { Id = 1, Status = PackageStatus.Ordered, BoxId = 10 };
        _packageRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(package);

        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.AcceptPackageAsync(1, 5, 99));
    }

    [Fact]
    public async Task ProcessExpiredPackagesAsync_ShouldExpirePackages()
    {
        var expiredPackage = new Package { Id = 1, Status = PackageStatus.Accepted };
        typeof(Package).GetProperty(nameof(Package.ExpiredAt))?.SetValue(expiredPackage, DateTime.UtcNow.AddHours(-1));

        _packageRepoMock.Setup(r => r.GetPackagesRequiringExpirationAsync())
            .ReturnsAsync(new List<Package> { expiredPackage });

        await _service.ProcessExpiredPackagesAsync();

        Assert.Equal(PackageStatus.Expired, expiredPackage.Status);
        _packageRepoMock.Verify(r => r.UpdateAsync(expiredPackage), Times.Once);
        _notificationMock.Verify(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
    }
}
