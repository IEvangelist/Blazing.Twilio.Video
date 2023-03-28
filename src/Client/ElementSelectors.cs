// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client;

/// <summary>
/// Various well known HTML element <c>id</c> values used for JS interop calls.
/// </summary>
internal static class ElementSelectors
{
    /// <summary>
    /// HTML element <c>id</c> attribute value used to
    /// call <c>document.querySelector("#camera-preview")</c> on JS interop calls.
    /// </summary>
    internal const string CameraPreviewId = "#camera-preview";

    /// <summary>
    /// HTML element <c>id</c> attribute value used to
    /// call <c>document.querySelector("#participant-1")</c> on JS interop calls.
    /// </summary>
    internal const string ParticipantOneId = "#participant-1";

    /// <summary>
    /// HTML element <c>id</c> attribute value used to
    /// call <c>document.querySelector("#participant-2")</c> on JS interop calls.
    /// </summary>
    internal const string ParticipantTwoId = "#participant-2";

    /// <summary>
    /// Gets the current <paramref name="selector"/> as a descendent selector,
    /// formatted as <c>"<paramref name="selector"/> &gt; video"</c>.
    /// </summary>
    internal static string AsVideoSelector(this string selector) => $"""
        {selector} > video
        """;
}
