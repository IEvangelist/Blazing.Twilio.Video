// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Interop;

/// <summary>
/// The JavaScript interop <c>blazing-video.js</c> module representation.
/// </summary>
internal sealed partial class SiteJavaScriptModule
{
    /// <summary>
    /// Gets an array of video devices from the browser as either an
    /// empty array string, or string value that JSON deserializes into
    /// an array of <see cref="Device"/>.
    /// </summary>
    /// <returns>An array of <see cref="Device"/> objects as a JSON string.</returns>
    /// <remarks>This will never return <c>null</c>, instead it will
    /// return an empty array <see cref="Device[]"/>.</remarks>
    [JSImport("requestVideoDevices", nameof(SiteJavaScriptModule))]
    public static partial Task<string> RequestVideoDevicesAsync();

    /// <summary>
    /// Gets an array of video devices from the current browser.
    /// </summary>
    /// <returns>An array of <see cref="Device"/> objects.</returns>
    /// <remarks>This will never return <c>null</c>, instead it will
    /// return an empty array <see cref="Device[]"/>.</remarks>
    [JSImport("startVideo", nameof(SiteJavaScriptModule))]
    public static partial Task<bool> StartVideoAsync(string? deviceId, string? selector);

    /// <summary>
    /// Stop video, detach underlying HTML <c><video></video></c> element.
    /// </summary>
    [JSImport("stopVideo", nameof(SiteJavaScriptModule))]
    public static partial void StopVideo();

    /// <summary>
    /// Asynchronously creates or joins a room with the given <paramref name="roomName"/> and
    /// <paramref name="token"/>.
    /// </summary>
    /// <param name="roomName">The name of the room.</param>
    /// <param name="token">The token used to create or join the named room.</param>
    /// <returns><c>true</c> if created or joined, otherwise <c>false</c>.</returns>
    //createOrJoinRoom
    [JSImport("createOrJoinRoom", nameof(SiteJavaScriptModule))]
    public static partial Task<bool> CreateOrJoinRoomAsync(string roomName, string token);

    /// <summary>
    /// Leave the current contextual room.
    /// </summary>
    [JSImport("leaveRoom", nameof(SiteJavaScriptModule))]
    public static partial bool LeaveRoom();

    /// <summary>
    /// Logs a message to the native browser's <c>console.log</c>
    /// method, passing styles as additional parameters.
    /// </summary>
    /// <param name="message">Message or message template.</param>
    /// <param name="args">The various style arguments.</param>
    [JSImport("globalThis.console.log", nameof(SiteJavaScriptModule))]
    public static partial void LogInfo(
        string message,
        [JSMarshalAs<JSType.Array<JSType.Any>>] object?[] args);

    /// <summary>
    /// Using the given <paramref name="selector"/>, for example; <c>"#participant-1 > video"</c>
    /// id-based selector for the corresponding HTML video element that we
    /// want to enter picture in picture (PiP) mode.
    /// </summary>
    /// <param name="selector">The id-based selector for the HTML video element.</param>
    /// <param name="onSuccess">The callback that's invoked when the request set PiP mode.</param>
    /// <param name="onExited">The callback that's invoked when PiP has exited.</param>
    /// <returns><c>true</c> when the request was successfully sent
    /// to the corresponding <c>HTMLVideoElement</c> instance, else <c>false</c>.</returns>
    [JSImport("requestPictureInPicture", nameof(SiteJavaScriptModule))]
    public static partial Task<bool> RequestPictureInPictureAsync(
        string selector,
        [JSMarshalAs<JSType.Function<JSType.Boolean>>] Action<bool> onSuccess,
        [JSMarshalAs<JSType.Function>] Action onExited);

    /// <summary>
    /// Calls the following corresponding JavaScript
    /// functionality to exit picture in picture (PiP) mode.
    /// </summary>
    /// <param name="onExited">The callback that's invoked when PiP is exited.</param>
    [JSImport("exitPictureInPicture", nameof(SiteJavaScriptModule))]
    public static partial Task ExitPictureInPictureAsync(
        [JSMarshalAs<JSType.Function<JSType.Boolean>>] Action<bool> onExited);

    /// <summary>
    /// Calls the client browser's <c>navigator.clipboard.writeText</c> function, saving
    /// the given <paramref name="text"/> value to the clipboard.
    /// </summary>
    /// <param name="text">The text value to copy to the client's clipboard.</param>
    [JSImport("navigator.clipboard.writeText", nameof(SiteJavaScriptModule))]
    public static partial Task CopyToClipboardAsync(string text);
}
