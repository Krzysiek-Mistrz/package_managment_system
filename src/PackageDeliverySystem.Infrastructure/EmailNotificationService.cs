using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using PackageDeliverySystem.Core;

namespace PackageDeliverySystem.Infrastructure;

/// <summary>
/// sends notificatiobs by email through smtp relay
/// </summary>
public class EmailNotificationService : INotificationService
{
    private const string FromAddress = "noreply@packagedeliverysystem.local";
    private const string DemoRecipientAddress = "demo-recipient@packagedeliverysystem.local";
    private const string SmtpHost = "localhost";
    private const int SmtpPort = 1025;

    public async Task NotifyAsync(string userId, string message)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(FromAddress));
        email.To.Add(MailboxAddress.Parse(DemoRecipientAddress));
        email.Subject = $"Package Delivery System Notification ({userId})";
        email.Body = new TextPart("plain") { Text = message };

        using var client = new SmtpClient();
        await client.ConnectAsync(SmtpHost, SmtpPort, SecureSocketOptions.None);
        await client.SendAsync(email);
        await client.DisconnectAsync(true);
    }
}