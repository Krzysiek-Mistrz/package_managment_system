using Hangfire;
using PackageDeliverySystem.Core;

namespace PackageDeliverySystem.Infrastructure;

/// <summary>
/// defers email sending 2 hangfire background job
/// </summary>
public class BackgroundNotificationService : INotificationService
{
    public Task NotifyAsync(string userId, string message)
    {
        // Enqueue the email notification to be sent in another thread & run NotifyAsync on service
        BackgroundJob.Enqueue<EmailNotificationService>(s => s.NotifyAsync(userId, message));
        return Task.CompletedTask;
    }
}