using System.Threading.Tasks;

namespace Refundeo.Core.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string topic, string message);
    }
}
