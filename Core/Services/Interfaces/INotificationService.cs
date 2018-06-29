using System.Threading.Tasks;

namespace Refundeo.Core.Services.Interfaces
{
    public interface INotificationService
    {
        void SendNotification(string topic, string message);
        Task SendNotificationAsync(string topic, string message);
    }
}
