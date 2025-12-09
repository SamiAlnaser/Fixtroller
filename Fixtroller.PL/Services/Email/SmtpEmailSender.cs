using Fixtroller.BLL.Services.NotificationServices;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Fixtroller.PL.Services.Email
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

    public class SmtpEmailSender : IAppEmailSender
    {
        private readonly EmailSettings _settings;

        public SmtpEmailSender(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendAsync(
            string to,
            string subject,
            string body,
            CancellationToken ct = default)
        {
            // حماية بسيطة لو الإعدادات ناقصة أو to فاضية
            if (string.IsNullOrWhiteSpace(to) ||
                string.IsNullOrWhiteSpace(_settings.From) ||
                string.IsNullOrWhiteSpace(_settings.SmtpHost))
            {
                // ممكن تحط Log هنا لو حاب
                return; // ما نبعت إيميل، بس كمان ما نكسر الـ API
            }

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.UseSsl,
                Credentials = new NetworkCredential(_settings.UserName, _settings.Password)
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(_settings.From, _settings.DisplayName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(to);

            await client.SendMailAsync(mail);
        }
    }
}
