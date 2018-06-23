using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Services.Interfaces;

namespace Refundeo.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly IOptions<EmailAccountOptions> _emailAccountOptionsAccessor;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IOptions<StorageAccountOptions> _storageAccountOptionsAccessor;

        public EmailService(IOptions<EmailAccountOptions> emailAccountOptionsAccessor,
            IOptions<StorageAccountOptions> storageAccountOptionsAccessor, IHostingEnvironment hostingEnvironment,
            IBlobStorageService blobStorageService)
        {
            _emailAccountOptionsAccessor = emailAccountOptionsAccessor;
            _storageAccountOptionsAccessor = storageAccountOptionsAccessor;
            _blobStorageService = blobStorageService;
            _smtpClient = new SmtpClient
            {
                Host = _emailAccountOptionsAccessor.Value.Host,
                Port = _emailAccountOptionsAccessor.Value.Port,
                EnableSsl = _emailAccountOptionsAccessor.Value.EnableSsl,
                Credentials = new NetworkCredential(_emailAccountOptionsAccessor.Value.Email,
                    _emailAccountOptionsAccessor.Value.Password)
            };
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task SendMailAsync(string subject, string body, string receiverEmail)
        {
            using (var message = new MailMessage(_emailAccountOptionsAccessor.Value.Email, receiverEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
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

            var template = await GetVatFormMailTemplateAsync();

            await SendMailAsync(
                $"Refundeo - Tax Free Form - {merchantInformation.CompanyName} {refundDate}",
                template,
                receiverEmail);
        }

        private async Task<string> GetVatFormMailTemplateAsync()
        {
            var blob = await _blobStorageService.DownloadAsync(
                _storageAccountOptionsAccessor.Value.EmailTemplatesContainerNameOption, "VATFormMailTemplate.html");

            return System.Text.Encoding.UTF8.GetString(blob.ToArray());
        }
    }
}
