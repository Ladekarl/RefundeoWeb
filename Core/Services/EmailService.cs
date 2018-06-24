﻿using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.Options;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Services.Interfaces;
using Rotativa.AspNetCore;

namespace Refundeo.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly IOptions<EmailAccountOptions> _emailAccountOptionsAccessor;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IHostingEnvironment _hostingEnvironment;

        private readonly IOptions<StorageAccountOptions> _storageAccountOptionsAccessor;

        private readonly INodeServices _nodeServices;

        public EmailService(IOptions<EmailAccountOptions> emailAccountOptionsAccessor,
            IOptions<StorageAccountOptions> storageAccountOptionsAccessor,
            IBlobStorageService blobStorageService, IHostingEnvironment hostingEnvironment,
            INodeServices nodeServices)
        {
            _emailAccountOptionsAccessor = emailAccountOptionsAccessor;
            _storageAccountOptionsAccessor = storageAccountOptionsAccessor;
            _blobStorageService = blobStorageService;
            _hostingEnvironment = hostingEnvironment;
            _smtpClient = new SmtpClient
            {
                Host = _emailAccountOptionsAccessor.Value.Host,
                Port = _emailAccountOptionsAccessor.Value.Port,
                EnableSsl = _emailAccountOptionsAccessor.Value.EnableSsl,
                Credentials = new NetworkCredential(_emailAccountOptionsAccessor.Value.Email,
                    _emailAccountOptionsAccessor.Value.Password)
            };
            _nodeServices = nodeServices;
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

        public async Task SendVATMailAsync(ControllerContext controllerContext, RefundCase refundCase,
            string receiverEmail)
        {
            var refundDate =
                $"{refundCase.DateCreated.Day}/{refundCase.DateCreated.Month}/{refundCase.DateCreated.Year}";

            var merchantInformation = refundCase.MerchantInformation;

            var template = await GetVatFormMailTemplateAsync();
            var vatForm = await GetVatFormAsync(controllerContext, refundCase);
            var attachment = new Attachment(vatForm, GetVatFormName(refundCase), MediaTypeNames.Application.Pdf);

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
            return $"{refundCase.MerchantInformation.CompanyName} {refundDate} {refundCase.Id}";
        }

        private async Task<Stream> GetVatFormAsync(ActionContext controllerContext, RefundCase refundCase)
        {
            var blob = await _blobStorageService.DownloadAsync(
                _storageAccountOptionsAccessor.Value.EmailTemplatesContainerNameOption, "VATFormTemplate.html");

            var htmlContent = System.Text.Encoding.UTF8.GetString(blob.ToArray());

            var pdf = new ViewAsPdf("VATForm")
            {
                FileName = "test"
            };

            var pdfData = await pdf.BuildFile(controllerContext);

            return new MemoryStream(pdfData);
        }
    }
}
