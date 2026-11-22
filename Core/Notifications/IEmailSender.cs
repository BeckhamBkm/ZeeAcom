namespace ZeeAcom.Core.Notifications;

public interface IEmailSender
{
    /// <summary>Sends a plain-text email. Returns false if it wasn't sent; the caller decides what that means.</summary>
    Task<bool> SendAsync(string to, string subject, string body, string? replyTo, CancellationToken cancellationToken);
}
