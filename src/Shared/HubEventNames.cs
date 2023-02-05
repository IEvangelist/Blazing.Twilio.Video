// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Shared;

public static class HubEventNames
{
    /// <summary>
    /// The name of the event that is triggered when the list of rooms has been updated.
    /// </summary>
    public const string RoomsUpdated = nameof(RoomsUpdated);

    /// <summary>
    /// The name of the event that is triggered when a user connects to the server's hub.
    /// </summary>
    public const string UserConnected = nameof(UserConnected);
}
