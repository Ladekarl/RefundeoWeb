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

        public async Task SendNotificationAsync(string topic, string message)
        {
            using (var client = new FcmClient(_settings))
            {
                var data = new Dictionary<string, string>()
                {
                    {"message", message}
                };

                var fcmMessage = new FcmMessage
                {
                    ValidateOnly = false,
                    Message = new Message
                    {
                        Topic = topic,
                        Data = data
                    }
                };

                var cts = new CancellationTokenSource();
                await client.SendAsync(fcmMessage, cts.Token);
            }
        }
    }
}
