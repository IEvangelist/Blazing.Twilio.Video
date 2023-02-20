// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Interop;

/// <summary>
/// The JavaScript interop <c>site.js</c> module representation.
/// </summary>
public interface ISiteVideoJavaScriptModule
{
    /// <summary>
    /// Starts a task that asynchronously Initializes the <c>site.js</c> module.
    /// Subsequent calls will use a cached reference to the module, and it only
    /// is initialized once.
    /// </summary>
    ValueTask InitializeModuleAsync();

    /// <summary>
    /// Asynchronously creates or joins a room with the given <paramref name="roomName"/> and
    /// <paramref name="token"/>.
    /// </summary>
    /// <param name="roomName">The name of the room.</param>
    /// <param name="token">The token used to create or join the named room.</param>
    /// <returns><c>true</c> if created or joined, otherwise <c>false</c>.</returns>
    ValueTask<bool> CreateOrJoinRoomAsync(string roomName, string token);

    /// <summary>
    /// Gets an array of video devices from the current browser.
    /// </summary>
    /// <returns>An array of <see cref="Device"/> objects.</returns>
    /// <remarks>This will never return <c>null</c>, instead it will
    /// return an empty array <see cref="Device[]"/>.</remarks>
    ValueTask<Device[]> GetVideoDevicesAsync();

    /// <summary>
    /// Leave the current contextual room.
    /// </summary>
    bool LeaveRoom();

    /// <summary>
    /// Asynchronously starts video for the given <paramref name="deviceId"/>,
    /// <paramref name="selector"/>, and <paramref name="token"/>.
    /// </summary>
    /// <param name="deviceId">The device identifier to start.</param>
    /// <param name="selector">The DOM selector for to start video on.</param>
    /// <param name="token">The <see cref="CancellationToken"/> used to signal cancellation.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> where <c>TResult</c>
    /// is <see cref="bool"/> (<c>true</c> or <c>false</c>).</returns>
    ValueTask<bool> StartVideoAsync(
        string? deviceId,
        string? selector,
        CancellationToken token = default);

    /// <summary>
    /// Stop video, detach underlying HTML <c><video></video></c> element.
    /// </summary>
    void StopVideo();

    /// <summary>
    /// Logs a message to the native browser's <c>console.log</c>
    /// method, passing styles as additional parameters.
    /// </summary>
    /// <param name="message">Message or message template.</param>
    /// <param name="args">The various style arguments.</param>
    void LogInfo(string message, params object?[] args);
}