using Blazing.Twilio.WasmVideo.Shared;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Blazing.Twilio.WasmVideo.Server.Hubs
{
    public class NotificationHub : Hub
    {
        public Task RoomAdded(string room) =>
            Clients.All.SendAsync(HubEndpoints.RoomAdded, room);
    }
}
