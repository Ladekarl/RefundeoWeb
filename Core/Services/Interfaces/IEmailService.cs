using System.Threading.Tasks;

namespace Refundeo.Core.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendMailAsync(string subject, string body, string receiverEmail);
    }
}
