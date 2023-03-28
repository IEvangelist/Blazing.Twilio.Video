// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Shared;

/// <summary>
/// Represents the various states in which the requesting of native browser devices can be.
/// </summary>
public enum RequestCameraState
{
    /// <summary>Currently requesting camera devices.</summary>
    RequestingCameras,

    /// <summary>Successfully requested and received camera devices.</summary>
    FoundCameras,

    /// <summary>Unable to get camera devices.</summary>
    Error
};
