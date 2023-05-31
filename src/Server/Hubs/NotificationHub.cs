// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Server.Hubs;

public sealed class NotificationHub : Hub
{
    /// <summary>
    /// Call to send a message to connected clients when rooms have been updated,
    /// and the given <paramref name="room"/> was the most recent addition.
    /// </summary>
    public Task RoomsUpdated(string room) =>
        Clients.All.SendAsync(HubEventNames.RoomsUpdated, room);

    /// <inheritdoc cref="Hub.OnConnectedAsync" />
    public override Task OnConnectedAsync() =>
        Clients.All.SendAsync(HubEventNames.UserConnected,
            "Add new user is visiting the Blazing Video Chat (Powered by Twilio) app");
}
