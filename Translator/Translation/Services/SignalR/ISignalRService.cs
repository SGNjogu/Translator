using System;
using System.Threading.Tasks;
using Translation.Models;

namespace Translation.Services.SignalR
{
    public interface ISignalRService
    {
        string ConnectionId { get; }

        event Action<SignalRTranslateMessage> SignalRMessageReceived;

        Task ConnectSignalR();
        Task DisconnectSignalR();
        Task SendSignalRMessage(SignalRTranslateMessage signalRTranslateMessage);
    }
}