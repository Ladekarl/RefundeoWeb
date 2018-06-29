using System.Threading.Tasks;
using FcmSharp.Responses;

namespace Refundeo.Core.Services.Interfaces
{
    public interface INotificationService
    {
        void SendNotification(string topic, string title, string message);
        Task<FcmMessageResponse> SendNotificationAsync(string topic, string title, string message);
    }
}
