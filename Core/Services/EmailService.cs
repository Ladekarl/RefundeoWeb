using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Models.RefundCase;
using Refundeo.Core.Services.Interfaces;

namespace Refundeo.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly IOptions<EmailAccountOptions> _emailAccountOptionsAccessor;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IConverter _converter;
        private readonly IUtilityService _utilityService;
        private readonly IOptions<StorageAccountOptions> _storageAccountOptionsAccessor;
        private readonly RefundeoDbContext _context;
        private readonly UserManager<RefundeoUser> _userManager;

        public EmailService(IOptions<EmailAccountOptions> emailAccountOptionsAccessor,
            IOptions<StorageAccountOptions> storageAccountOptionsAccessor, RefundeoDbContext context,
            UserManager<RefundeoUser> userManager, IBlobStorageService blobStorageService,
            IConverter converter, IUtilityService utilityService)
        {
            _emailAccountOptionsAccessor = emailAccountOptionsAccessor;
            _storageAccountOptionsAccessor = storageAccountOptionsAccessor;
            _context = context;
            _userManager = userManager;
            _blobStorageService = blobStorageService;
            _converter = converter;
            _utilityService = utilityService;
            _smtpClient = new SmtpClient
            {
                Host = _emailAccountOptionsAccessor.Value.Host,
                Port = _emailAccountOptionsAccessor.Value.Port,
                EnableSsl = _emailAccountOptionsAccessor.Value.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailAccountOptionsAccessor.Value.Email,
                    _emailAccountOptionsAccessor.Value.Password)
            };
        }

        public async Task<string> SendPasswordRecoveryMailAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null) return null;

            var isMerchant = await _userManager.IsInRoleAsync(user, RefundeoConstants.RoleMerchant);

            string email;

            if (isMerchant)
            {
                email = await _context.MerchantInformations
                    .Include(m => m.Merchants)
                    .Where(m => m.Merchants.Any(x => x.Id == user.Id))
                    .Select(m => m.AdminEmail)
                    .FirstOrDefaultAsync();
            }
            else
            {
                email = await _context.CustomerInformations.Where(c => c.CustomerId == user.Id)
                    .Select(c => c.Email)
                    .FirstOrDefaultAsync();
            }

            if (string.IsNullOrEmpty(email)) return null;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            if (string.IsNullOrEmpty(token)) return null;

            var body =
                $"Hello {user.UserName}<br/><br/>Please follow this link to reset your password: <a href=\"https://www.app.refundeo.com/resetpassword/{token}\">reset password</a><br/><br/>Best Regards<br/>Refundeo";

            await SendMailAsync("Reset your password", body, email, true);

            return email;
        }

        public async Task SendMailAsync(string subject, string body, string receiverEmail, bool isHtml,
            Attachment attachment = null)
        {
            if (attachment != null)
            {
                using (var message = new MailMessage(_emailAccountOptionsAccessor.Value.Email, receiverEmail)
                {
                    Subject = subject,
                    Body = body,
                    From = new MailAddress(_emailAccountOptionsAccessor.Value.Email, "Refundeo"),
                    IsBodyHtml = isHtml,
                    Attachments = {attachment}
                })
                {
                    await _smtpClient.SendMailAsync(message);
                }
            }
            else
            {
                using (var message = new MailMessage(_emailAccountOptionsAccessor.Value.Email, receiverEmail)
                {
                    Subject = subject,
                    Body = body,
                    From = new MailAddress(_emailAccountOptionsAccessor.Value.Email, "Refundeo"),
                    IsBodyHtml = isHtml
                })
                {
                    await _smtpClient.SendMailAsync(message);
                }
            }
        }

        public void SendVATMail(ControllerContext controllerContext, RefundCase refundCase,
            string receiverEmail)
        {
            SendVATMailAsync(controllerContext, refundCase, receiverEmail);
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

            return Encoding.UTF8.GetString(blob.ToArray());
        }

        private static string GetVatFormName(RefundCase refundCase)
        {
            var refundDate =
                $"{refundCase.DateCreated.Day}-{refundCase.DateCreated.Month}-{refundCase.DateCreated.Year}";
            return $"{refundCase.MerchantInformation.CompanyName} {refundDate} {refundCase.Id}";
        }

        private async Task<Stream> GetVatFormAsync(ActionContext controllerContext, RefundCase refundCase)
        {
            var model = new VatFormModel
            {
                Amount = $"{refundCase.Amount} {refundCase.MerchantInformation.Currency}",
                CustomerAddres = refundCase.CustomerInformation.Address.StreetName + " " +
                                 refundCase.CustomerInformation.Address.StreetNumber,
                CustomerCity = refundCase.CustomerInformation.Address.City,
                CustomerCountry = refundCase.CustomerInformation.Country,
                CustomerEmail = refundCase.CustomerInformation.Email,
                CustomerName = refundCase.CustomerInformation.FirstName + " " + refundCase.CustomerInformation.LastName,
                CustomerPassport = refundCase.CustomerInformation.Passport,
                CustomerPhone = refundCase.CustomerInformation.Phone,
                CustomerPostalCode = refundCase.CustomerInformation.Address.PostalCode,
                Date = $"{refundCase.DateCreated.Day}/{refundCase.DateCreated.Month}/{refundCase.DateCreated.Year}",
                MerchantAddres = refundCase.MerchantInformation.Address.StreetName + " " +
                                 refundCase.MerchantInformation.Address.StreetNumber,
                MerchantCity = refundCase.MerchantInformation.Address.City,
                MerchantCountry = refundCase.MerchantInformation.Address.Country,
                MerchantEmail = refundCase.MerchantInformation.ContactEmail,
                MerchantName = refundCase.MerchantInformation.CompanyName,
                MerchantPhone = refundCase.MerchantInformation.ContactPhone,
                MerchantPostalCode = refundCase.MerchantInformation.Address.PostalCode,
                MerchantVatNo = refundCase.MerchantInformation.VATNumber,
                ReceiptNumber = refundCase.ReceiptNumber,
                RefundAmount = $"{refundCase.RefundAmount} {refundCase.MerchantInformation.Currency}",
                VatAmount = $"{refundCase.Amount * 0.20} {refundCase.MerchantInformation.Currency}",
                CustomerSignature = await _utilityService.ConvertBlobPathToBase64Async(refundCase.CustomerSignature),
                MerchantSignature = await _utilityService.ConvertBlobPathToBase64Async(refundCase.MerchantSignature),
                QrCode = await _utilityService.ConvertBlobPathToBase64Async(refundCase.QRCode)
            };

            var html = await GetVatFormHtmlAsync(controllerContext, model);

            var doc = new HtmlToPdfDocument
            {
                GlobalSettings =
                {
                    ColorMode = ColorMode.Grayscale,
                    Orientation = Orientation.Portrait,
                    Margins = new MarginSettings {Top = 5, Bottom = 5},
                    PaperSize = PaperKind.A4
                },
                Objects =
                {
                    new ObjectSettings
                    {
                        HtmlContent = html,
                        WebSettings =
                        {
                            DefaultEncoding = "utf-8",
                            MinimumFontSize = 12
                        }
                    }
                }
            };

            return new MemoryStream(_converter.Convert(doc));
        }

        private async Task<string> GetVatFormHtmlAsync(ActionContext context, VatFormModel model)
        {
            const string viewName = "VATForm";

            if (!(context.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) is ICompositeViewEngine
                engine))
            {
                return null;
            }

            var viewResult = engine.FindView(context, viewName, true);
            StringBuilder html;

            var tempDataProvider =
                context.HttpContext.RequestServices.GetService(typeof(ITempDataProvider)) as ITempDataProvider;

            var viewDataDictionary = new ViewDataDictionary(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary())
            {
                Model = model
            };

            using (var output = new StringWriter())
            {
                var view = viewResult.View;
                var tempDataDictionary = new TempDataDictionary(context.HttpContext, tempDataProvider);
                var viewContext = new ViewContext(
                    context,
                    viewResult.View,
                    viewDataDictionary,
                    tempDataDictionary,
                    output,
                    new HtmlHelperOptions());

                await view.RenderAsync(viewContext);

                html = output.GetStringBuilder();
            }

            return html.ToString();
        }
    }
}
