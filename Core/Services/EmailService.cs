﻿using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
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
        private readonly IBlobStorageService _blobStorageService;
        private readonly IOptions<StorageAccountOptions> _storageAccountOptionsAccessor;
        private readonly IConverter _converter;

        public EmailService(IOptions<EmailAccountOptions> emailAccountOptionsAccessor,
            IOptions<StorageAccountOptions> storageAccountOptionsAccessor,
            IBlobStorageService blobStorageService)
        {
            _converter = new SynchronizedConverter(new PdfTools());
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
        }

        public async Task SendMailAsync(string subject, string body, string receiverEmail, bool isHtml,
            Attachment attachment)
        {
            using (var message = new MailMessage(_emailAccountOptionsAccessor.Value.Email, receiverEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml,
                Attachments = {attachment}
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
            var vatForm = await GetVatFormAsync(refundCase);
            var attachment = new Attachment(vatForm, GetVatFormName(refundCase));

            await SendMailAsync(
                $"Refundeo - Tax Free Form - {merchantInformation.CompanyName} {refundDate}",
                template,
                receiverEmail, true, attachment);
        }

        private async Task<string> GetVatFormMailTemplateAsync()
        {
            var blob = await _blobStorageService.DownloadAsync(
                _storageAccountOptionsAccessor.Value.EmailTemplatesContainerNameOption, "VATFormMailTemplate.html");

            return System.Text.Encoding.UTF8.GetString(blob.ToArray());
        }

        private static string GetVatFormName(RefundCase refundCase)
        {
            var refundDate =
                $"{refundCase.DateCreated.Day}-{refundCase.DateCreated.Month}-{refundCase.DateCreated.Year}";
            return $"{refundCase.MerchantInformation.CompanyName}_{refundDate}_{refundCase.Id}";
        }

        private async Task<Stream> GetVatFormAsync(RefundCase refundCase)
        {
            var blob = await _blobStorageService.DownloadAsync(
                _storageAccountOptionsAccessor.Value.EmailTemplatesContainerNameOption, "VATFormTemplate.html");

            var htmlContent = System.Text.Encoding.UTF8.GetString(blob.ToArray());

            var doc = new HtmlToPdfDocument
            {
                GlobalSettings =
                {
                    ColorMode = ColorMode.Grayscale,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    DocumentTitle = GetVatFormName(refundCase)
                },
                Objects =
                {
                    new ObjectSettings
                    {
                        PagesCount = false,
                        HtmlContent = htmlContent,
                        WebSettings = {DefaultEncoding = "utf-8"}
                    }
                }
            };

            var pdf = _converter.Convert(doc);
            return new MemoryStream(pdf);
        }
    }
}
