using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Refundeo.Core.Data.Models;

namespace Refundeo.Core.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendMailAsync(string subject, string body, string receiverEmail, bool isHtml,
            Attachment attachment = null);

        Task<string> SendPasswordRecoveryMailAsync(string username);
        Task SendVATMailAsync(RefundCase refundCase, string receiverEmail);
    }
}
