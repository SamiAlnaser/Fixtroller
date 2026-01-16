using Fixtroller.BLL.Services.NotificationServices;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;


namespace Fixtroller.PL.Services.Notifications.Email
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = default!;
        public int SmtpPort { get; set; }
        public bool UseSsl { get; set; }
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string From { get; set; } = default!;
        public string DisplayName { get; set; } = "Fixtroller";
    }

    public sealed class SmtpEmailSender : IAppEmailSender
    {
        private readonly EmailSettings _settings;

        public SmtpEmailSender(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<bool> SendAsync(string to, string subject, string body, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(to) ||
                string.IsNullOrWhiteSpace(_settings.From) ||
                string.IsNullOrWhiteSpace(_settings.SmtpHost) ||
                _settings.SmtpPort <= 0)
                return false;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.DisplayName ?? "Fixtroller", _settings.From));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

            using var client = new SmtpClient();
            client.Timeout = 20000;

            var secure =
              _settings.SmtpPort == 465 ? SecureSocketOptions.SslOnConnect :
              SecureSocketOptions.StartTlsWhenAvailable;

            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, secure, ct);
            await client.AuthenticateAsync(_settings.UserName, _settings.Password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);

            return true;
        }
    }
}
