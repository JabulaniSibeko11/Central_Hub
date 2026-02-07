using Central_Hub.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Central_Hub.Services.Email
{
    public sealed class SmtpEmailService : IEmailService
    {
        private readonly EmailOptions _opt;
        private readonly ILogger<SmtpEmailService> _log;

        public SmtpEmailService(IOptions<EmailOptions> opt, ILogger<SmtpEmailService> log)
        {
            _opt = opt.Value;
            _log = log;
        }

        public async Task<bool> SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
        {
            if (!_opt.Enabled)
            {
                _log.LogInformation("Email disabled. Skipping send to {To}. Subject: {Subject}", toEmail, subject);
                return false;
            }

            var recipient = string.IsNullOrWhiteSpace(_opt.OverrideTo) ? toEmail : _opt.OverrideTo;

            if (string.IsNullOrWhiteSpace(_opt.Host))
            {
                _log.LogWarning("Email enabled but Host not configured. Skipping send.");
                return false;
            }

            try
            {
                using var msg = new MailMessage
                {
                    From = new MailAddress(_opt.FromAddress, _opt.FromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                msg.To.Add(recipient);

                using var smtp = new SmtpClient(_opt.Host, _opt.Port)
                {
                    EnableSsl = _opt.UseSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_opt.Username, _opt.Password)
                };

                // SmtpClient doesn’t support CancellationToken directly; respect ct by early check
                ct.ThrowIfCancellationRequested();

                await smtp.SendMailAsync(msg);

                _log.LogInformation("Email sent to {To}. Subject: {Subject}", recipient, subject);
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to send email to {To}. Subject: {Subject}", recipient, subject);
                return false; // don’t break main flow
            }
        }
    }
}
