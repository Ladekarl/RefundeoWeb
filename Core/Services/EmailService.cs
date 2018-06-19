using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Refundeo.Core.Helpers;
using Refundeo.Core.Services.Interfaces;

namespace Refundeo.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly IOptions<EmailAccountOptions> _optionsAccessor;

        public EmailService(IOptions<EmailAccountOptions> optionsAccessor)
        {
            _optionsAccessor = optionsAccessor;
            _smtpClient = new SmtpClient
            {
                Host = _optionsAccessor.Value.Host,
                Port = _optionsAccessor.Value.Port,
                EnableSsl = _optionsAccessor.Value.EnableSsl,
                Credentials = new NetworkCredential(_optionsAccessor.Value.Email, _optionsAccessor.Value.Password)
            };
        }

        public async Task SendMailAsync(string subject, string body, string receiverEmail)
        {
            using (var message = new MailMessage(_optionsAccessor.Value.Email, receiverEmail)
            {
                Subject = subject,
                Body = body
            })
            {
                await _smtpClient.SendMailAsync(message);
            }
        }
    }
}
