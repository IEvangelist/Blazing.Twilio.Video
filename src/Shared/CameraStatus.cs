// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Shared;

/// <summary>
/// An <see cref="enum"/> representing various camera status values.
/// </summary>
public enum CameraStatus
{
    /// <summary>The default, when the camera status is idle.</summary>
    Idle,

    /// <summary>The camera status when previewing the live
    /// feed, smile you're on camera.</summary>
    Previewing,

    /// <summary>Requesting a preview of the camera.</summary>
    RequestingPreview,

    /// <summary>The camera status when in a call.</summary>
    InCall
};
