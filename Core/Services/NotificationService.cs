using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FcmSharp;
using FcmSharp.Requests;
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

        public void SendNotification(string topic, string message)
        {
            using (var client = new FcmClient(_settings))
            {
                var fcmMessage = BuildMessage(topic, message);
                var cts = new CancellationTokenSource();
                client.SendAsync(fcmMessage, cts.Token).Start();
            }
        }

        public async Task SendNotificationAsync(string topic, string message)
        {
            using (var client = new FcmClient(_settings))
            {
                var fcmMessage = BuildMessage(topic, message);
                var cts = new CancellationTokenSource();
                await client.SendAsync(fcmMessage, cts.Token);
            }
        }

        private FcmMessage BuildMessage(string topic, string message)
        {
            var data = new Dictionary<string, string>
            {
                {"message", message}
            };

            return new FcmMessage
            {
                ValidateOnly = false,
                Message = new Message
                {
                    Topic = topic.Replace("-", string.Empty),
                    Data = data
                }
            };
        }
    }
}
