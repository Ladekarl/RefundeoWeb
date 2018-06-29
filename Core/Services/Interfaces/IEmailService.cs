using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Refundeo.Core.Data.Models;

namespace Refundeo.Core.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendMailAsync(string subject, string body, string receiverEmail, bool isHtml,
            Attachment attachment);
        void SendVATMail(ControllerContext controllerContext, RefundCase refundCase, string receiverEmail);
        Task SendVATMailAsync(ControllerContext controllerContext, RefundCase refundCase, string receiverEmail);
    }
}
