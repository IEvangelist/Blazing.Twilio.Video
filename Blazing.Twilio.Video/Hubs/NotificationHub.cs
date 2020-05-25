using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Blazing.Twilio.Video.Hubs
{
    public class NotificationHub : Hub
    {
        public Task RoomAdded(string room) =>
            Clients.Others.SendAsync(nameof(RoomAdded), room);
    }
}