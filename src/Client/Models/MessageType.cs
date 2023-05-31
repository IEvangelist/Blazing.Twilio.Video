// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Models;

public enum MessageType
{
    /// <summary> The user is selecting a camera.</summary>
    SelectCamera,

    /// <summary>
    /// The user is creating or joining a room,
    /// this event is accompanied with an <c><see cref="AppEventMessage.TwilioToken"/></c>
    /// that corresponds to the <c>local</c>
    /// </summary>
    CreateOrJoinRoom,

    /// <summary>The user is leaving the room.</summary>
    LeaveRoom
};