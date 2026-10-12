using PackageDeliverySystem.Core;

namespace PackageDeliverySystem.Infrastructure;

public class NotificationService : INotificationService
{
    /// <summary>
    /// sends a notification 2 user
    /// </summary>
    public Task NotifyAsync(string userId, string message)
    {
        // yeah just printing :)
        Console.WriteLine($"Notification to {userId}: {message}");
        return Task.CompletedTask;
    }
}