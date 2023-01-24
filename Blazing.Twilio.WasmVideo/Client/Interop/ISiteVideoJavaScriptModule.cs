// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.WasmVideo.Client.Interop;

/// <summary>
/// The JavaScript interop <c>site.js</c> module representation.
/// </summary>
public interface ISiteVideoJavaScriptModule
{
    /// <summary>
    /// Initializes the <c>site.js</c> module.
    /// </summary>
    ValueTask InitiailizeModuleAsync();

    /// <summary>
    /// Creates or joins a room with the given <paramref name="roomName"/> and
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
    ValueTask<Device[]> GetVideoDevicesAsync();

    /// <summary>
    /// Leave the current contextual room.
    /// </summary>
    ValueTask LeaveRoomAsync();

    /// <summary>
    /// Start video for the given <paramref name="deviceId"/> and <paramref name="selector"/>.
    /// </summary>
    /// <param name="deviceId">The device identifier to start.</param>
    /// <param name="selector">The DOM selector for to start videon on.</param>
    ValueTask StartVideoAsync(string deviceId, string selector);
}