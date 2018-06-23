using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        private readonly IHostingEnvironment _hostingEnvironment;

        public EmailService(IOptions<EmailAccountOptions> optionsAccessor, IHostingEnvironment hostingEnvironment)
        {
            _optionsAccessor = optionsAccessor;
            _smtpClient = new SmtpClient
            {
                Host = _optionsAccessor.Value.Host,
                Port = _optionsAccessor.Value.Port,
                EnableSsl = _optionsAccessor.Value.EnableSsl,
                Credentials = new NetworkCredential(_optionsAccessor.Value.Email, _optionsAccessor.Value.Password)
            };
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task SendMailAsync(string subject, string body, string receiverEmail)
        {
            using (var message = new MailMessage(_optionsAccessor.Value.Email, receiverEmail)
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

            var template = GetTemplate(Path.Combine(_hostingEnvironment.ContentRootPath, "VATFormMailTemplate.html"));

            var templateFormatted = string.Format(template,
                merchantInformation.CompanyName + " - " + merchantInformation.Address.City,
                refundDate);

            await SendMailAsync(
                $"Refundeo - Tax Free Form - {merchantInformation.CompanyName} {refundDate}",
                templateFormatted,
                receiverEmail);
        }

        private static string GetTemplate(string strFile)
        {
            string content;

            using (var sr = new StreamReader(strFile))
            {
                content = sr.ReadToEnd();
            }

            return content;
        }
    }
}
