using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Refundeo.Core.Data.Models;
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

        public async Task SendVATMailAsync(RefundCase refundCase, string receiverEmail)
        {
            var refundDate =
                $"{refundCase.DateCreated.Day}/{refundCase.DateCreated.Month}/{refundCase.DateCreated.Year}";

            var merchantInformation = refundCase.MerchantInformation;
            var customerInformation = refundCase.CustomerInformation;

            await SendMailAsync(
                $"Refundeo - Tax Free Form - {merchantInformation.CompanyName} {refundDate}",
                $"Dear {customerInformation.FirstName}\n\nPlease find your tax free form for your purchase at {merchantInformation.CompanyName} - {merchantInformation.Address.City} on {refundDate} attached to this email.\n\nThe VAT form should be filled and stamped at a local customs office.\n\nUpload the stamped form along with the original receipt in the app, to claim your refund.\n\nBest Regards\nRefundeo",
                receiverEmail);
        }
    }
}
