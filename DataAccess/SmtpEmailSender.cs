using System.Net;
using System.Net.Mail;
using ZeeAcom.Core.Notifications;

namespace ZeeAcom.DataAccess;

/// <summary>
/// Sends email through the SMTP server in configuration ("Smtp" section). When no SMTP host is
/// configured (e.g. local development) the email is written to the log instead of being sent.
/// Keep the SMTP password in user-secrets or environment variables, not appsettings.json.
/// </summary>
public sealed class SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task<bool> SendAsync(string to, string subject, string body, string? replyTo, CancellationToken cancellationToken)
    {
        var smtp = configuration.GetSection("Smtp");
        var host = smtp["Host"];
        var from = smtp["From"];

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
        {
            logger.LogInformation("Email not sent (no Smtp:Host/Smtp:From configured). To: {To} Subject: {Subject}{NewLine}{Body}",
                to, subject, Environment.NewLine, body);
            return false;
        }

        try
        {
            using var message = new MailMessage(from, to, subject, body);
            if (!string.IsNullOrWhiteSpace(replyTo))
            {
                message.ReplyToList.Add(replyTo);
            }

            using var client = new SmtpClient(host, smtp.GetValue("Port", 587))
            {
                EnableSsl = smtp.GetValue("EnableSsl", true),
                Credentials = string.IsNullOrWhiteSpace(smtp["UserName"])
                    ? null
                    : new NetworkCredential(smtp["UserName"], smtp["Password"]),
            };

            await client.SendMailAsync(message, cancellationToken);
            return true;
        }
        catch (Exception ex) when (ex is SmtpException or InvalidOperationException or FormatException)
        {
            logger.LogError(ex, "Could not send email to {To}", to);
            return false;
        }
    }
}
