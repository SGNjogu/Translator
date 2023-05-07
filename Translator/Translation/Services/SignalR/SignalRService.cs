using Microsoft.AppCenter.Crashes;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Translation.AuditTracking;
using Translation.Hmac;
using Translation.Interface;
using Translation.Models;

namespace Translation.Services.SignalR
{
    public class SignalRService : ISignalRService
    {
        private HubConnection _connection = null;
        public event Action<SignalRTranslateMessage> SignalRMessageReceived;
        public string ConnectionId { get; private set; } = string.Empty;

        private const string SignalREndpointBase = "hubs/translation";

        private readonly IAppCrashlytics _crashlytics = new AppCrashlytics();

        private void Initialize()
        {
            HttpClient client = HttpClientProvider.Create();

            _connection = new HubConnectionBuilder()
                .WithUrl($"{client.BaseAddress.AbsoluteUri}{SignalREndpointBase}")
                .Build();
        }

        public async Task ConnectSignalR()
        {
            try
            {
                if (_connection == null)
                    Initialize();

                await _connection.StartAsync();
                ConnectionId = _connection.ConnectionId;
                _connection.On<SignalRTranslateMessage>("receivedmesage", (signalRTranslateMessage) => SignalRMessageReceived?.Invoke(signalRTranslateMessage));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task DisconnectSignalR()
        {
            if(_connection != null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
            }
            _connection = null;
        }

        public async Task SendSignalRMessage(SignalRTranslateMessage signalRTranslateMessage)
        {
            try
            {
                signalRTranslateMessage.ConnectionId = _connection.ConnectionId;

                if (!string.IsNullOrEmpty(signalRTranslateMessage.ConnectionId))
                    await _connection.SendAsync("SendTranslationHubMessage", signalRTranslateMessage);
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
                throw ex;
            }

        }

    }
}
