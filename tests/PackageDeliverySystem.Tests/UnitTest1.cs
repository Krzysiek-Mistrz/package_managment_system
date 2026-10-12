using PackageDeliverySystem.Core;
using Xunit;

namespace PackageDeliverySystem.Tests;

public class PackageTests
{
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
}
