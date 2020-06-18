using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Blazing.Twilio.Video.Hubs
{
    public class NotificationHub : Hub
    {
        internal const string Endpoint = "/notificationHub";

        internal const string RoomAddedRoute = nameof(RoomAdded);

        public Task RoomAdded(string room) =>
            Clients.All.SendAsync(RoomAddedRoute, room);
    }
}