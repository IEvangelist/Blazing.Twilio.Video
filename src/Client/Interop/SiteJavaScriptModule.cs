﻿// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Interop;

/// <summary>
/// The JavaScript interop <c>site.js</c> module representation.
/// </summary>
internal sealed partial class SiteJavaScriptModule
{
    /// <summary>
    /// Gets an array of video devices from the browser.
    /// </summary>
    /// <returns>An array of <see cref="Device"/> objects.</returns>
    /// <remarks>This will never return <c>null</c>, instead it will
    /// return an empty array <see cref="Device[]"/>.</remarks>
    [JSImport("requestVideoDevices", nameof(SiteJavaScriptModule))]
    [return: JSMarshalAs<JSType.Promise<JSType.String>>]
    public static partial Task<string> RequestVideoDevicesAsync();

    /// <summary>
    /// Gets an array of video devices from the current browser.
    /// </summary>
    /// <returns>An array of <see cref="Device"/> objects.</returns>
    /// <remarks>This will never return <c>null</c>, instead it will
    /// return an empty array <see cref="Device[]"/>.</remarks>
    [JSImport("startVideo", nameof(SiteJavaScriptModule))]
    [return: JSMarshalAs<JSType.Promise<JSType.Boolean>>]
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
    [JSImport("log", nameof(SiteJavaScriptModule))]
    public static partial void LogInfo(
        string message,
        [JSMarshalAs<JSType.Array<JSType.Any>>] object?[] args);

    /// <summary>
    /// Gets a value indicating whether the client supports picture in picture (PiP).
    /// </summary>
    /// <param name="videoElementId">The <c>id</c> attribute value for the HTML video
    /// element, for example; <c>"participant-1"</c></param>
    /// <returns><c>true</c> when <c>document.pictureInPictureEnabled</c> or there is
    /// a <c>video</c> HTML element and it hasn't disabled PiP, else <c>false</c>.</returns>
    [JSImport("isPictureInPictureSupported", nameof(SiteJavaScriptModule))]
    public static partial bool IsPictureInPictureSupported(string videoElement);

    /// <summary>
    /// Using the given <paramref name="videoElementId"/>, for example; <c>"participant-1"</c>
    /// id-based selector for the corresponding HTML video element that we
    /// want to enter picture in picture (PiP) mode.
    /// </summary>
    /// <param name="videoElementId">The id-based selector for the HTML video element.</param>
    /// <returns><c>true</c> when the request was successfully sent
    /// to the corresponding <c>HTMLVideoElement</c> instance, else <c>false</c>.</returns>
    [JSImport("requestPictureInPicture", nameof(SiteJavaScriptModule))]
    public static partial Task<bool> RequestPictureInPictureAsync(string videoElement);

    /// <summary>
    /// Calls the following corresponding JavaScript
    /// functionality to exit picture in picture (PiP) mode.
    /// </summary>
    [JSImport("exitPictureInPicture", nameof(SiteJavaScriptModule))]
    public static partial Task ExitPictureInPictureAsync();

    /// <summary></summary>
    /// <param name="videoElementId"></param>
    partial void OnVideoElementEnteredPictureInPicture(string videoElementId);

    /// <summary></summary>
    /// <param name="videoElementId"></param>
    partial void OnVideoElementLeavePictureInPicture(string videoElementId);
}