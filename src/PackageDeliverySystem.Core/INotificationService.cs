namespace PackageDeliverySystem.Core;

public interface INotificationService
{
    Task NotifyAsync(string userId, string message);
}
