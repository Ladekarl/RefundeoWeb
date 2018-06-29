using System.Threading.Tasks;
using FcmSharp.Responses;

namespace Refundeo.Core.Services.Interfaces
{
    public interface INotificationService
    {
        Task<FcmMessageResponse> SendNotificationAsync(string topic, string title, string message);
    }
}
