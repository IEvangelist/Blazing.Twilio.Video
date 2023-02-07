// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client;

/// <summary>
/// Various well known HTML element <c>id</c> values used for JS interop calls.
/// </summary>
internal static class ElementIds
{
    /// <summary>
    /// HTML element <c>id</c> attribute value used to
    /// call <c>document.querySelector("#camera-preview")</c> on JS interop calls.
    /// </summary>
    internal const string CameraPreview = "#camera-preview";

    /// <summary>
    /// HTML element <c>id</c> attribute value used to
    /// call <c>document.querySelector("#participant-1")</c> on JS interop calls.
    /// </summary>
    internal const string ParticipantOne = "#participant-1";

    /// <summary>
    /// HTML element <c>id</c> attribute value used to
    /// call <c>document.querySelector("#participant-2")</c> on JS interop calls.
    /// </summary>
    internal const string ParticipantTwo = "#participant-2";
}
