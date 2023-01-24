// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.WasmVideo.Server.Hubs;

public sealed class NotificationHub : Hub
{
    public Task RoomsUpdated(string room) =>
        Clients.All.SendAsync(HubEndpoints.RoomsUpdated, room);
}
