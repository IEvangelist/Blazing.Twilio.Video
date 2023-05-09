// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Pages;

public sealed partial class Index
{
    /// <summary>The calculated classes for the <c>video</c> HTML elements.</summary>
    string MidCameraClass => HideHeader ? "min-vh-85" : "min-vh-55";

    /// <summary>The calculated class for the grid header with the Blazor hearts emoji Twilio.</summary>
    string HeaderClass => HideHeader ? "hidden" : "";

    /// <summary>Hide the header when either single size column is not maximized.</summary>
    bool HideHeader => ParticipantOneColumnSize is 10 || ParticipantTwoColumnSize is 10;

    /// <summary>The "#participant-1 > video" HTML element column size.</summary>
    int ParticipantOneColumnSize => AppState.CameraStatus switch
    {
        CameraStatus.PictureInPicture => 0,

        CameraStatus.Idle or
        CameraStatus.InCall or
        CameraStatus.RequestingPreview or
        CameraStatus.Previewing => 5,

        _ => 10
    };

    /// <summary>Is <c>"hidden"</c> when <see cref="ParticipantOneColumnSize"/> is <c>0</c>.
    /// Otherwise, <see cref="MidCameraClass"/> is returned.</summary>
    string ParticipantOneClass => ParticipantOneColumnSize switch
    {
        0 => "hidden",
        _ => MidCameraClass
    };

    /// <summary>The "#participant-2 > video" HTML element column size.</summary>
    int ParticipantTwoColumnSize => AppState.CameraStatus switch
    {
        CameraStatus.PictureInPicture => 10,

        CameraStatus.Idle or
        CameraStatus.InCall or
        CameraStatus.RequestingPreview or
        CameraStatus.Previewing => 5,

        _ => 0
    };

    /// <summary>Is <c>"hidden"</c> when <see cref="ParticipantTwoColumnSize"/> is <c>0</c>.
    /// Otherwise, <see cref="MidCameraClass"/> is returned.</summary>
    string ParticipantTwoClass => ParticipantTwoColumnSize switch
    {
        0 => "hidden",
        _ => MidCameraClass
    };
}
