using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FcmSharp;
using FcmSharp.Requests;
using FcmSharp.Responses;
using FcmSharp.Settings;
using Microsoft.Extensions.Configuration;
using Refundeo.Core.Services.Interfaces;

namespace Refundeo.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly FcmClientSettings _settings;

        public NotificationService(IConfiguration Configuration)
        {
            var fcmKey = Configuration["fcmkey"];
            var projectId = Configuration["FirebaseProjectId"];
            _settings = new FcmClientSettings(projectId, fcmKey);
        }

        public async Task<FcmMessageResponse> SendNotificationAsync(string topic, string title, string message)
        {
            using (var client = new FcmClient(_settings))
            {
                var fcmMessage = BuildMessage(topic, title, message);
                var cts = new CancellationTokenSource();
                return await client.SendAsync(fcmMessage, cts.Token);
            }
        }

        private FcmMessage BuildMessage(string topic, string title, string message)
        {
            return new FcmMessage
            {
                ValidateOnly = false,
                Message = new Message
                {
                    Topic = topic.Replace("-", string.Empty),
                    Notification = new Notification
                    {
                        Title = title,
                        Body = message
                    }
                }
            };
        }
    }
}
