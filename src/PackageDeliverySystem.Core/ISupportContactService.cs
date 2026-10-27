namespace PackageDeliverySystem.Core;

/// <summary>
/// lets any user or courier reach support about a problem. routes through the same
/// notification pipeline as everything else (INotificationService), so it needs no
/// separate e-mail/SMTP wiring of its own
/// </summary>
public interface ISupportContactService
{
    Task ContactSupportAsync(int personId, string subject, string message);
}
