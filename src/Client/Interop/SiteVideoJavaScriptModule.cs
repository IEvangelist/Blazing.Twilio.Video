// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Interop;

/// <inheritdoc cref="ISiteVideoJavaScriptModule" />
internal sealed class SiteVideoJavaScriptModule : ISiteVideoJavaScriptModule
{
    private readonly IJSInProcessRuntime _javaScript;
    private IJSInProcessObjectReference? _siteModule;

    public SiteVideoJavaScriptModule(IJSInProcessRuntime javaScript) =>
        ArgumentNullException.ThrowIfNull(_javaScript = javaScript);

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.InitiailizeModuleAsync" />
    public async ValueTask InitiailizeModuleAsync() =>
        _siteModule ??=
            await _javaScript.InvokeAsync<IJSInProcessObjectReference>(
                "import", "./site.js");

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.GetVideoDevicesAsync" />
    public ValueTask<Device[]> GetVideoDevicesAsync() =>
        _siteModule?.InvokeAsync<Device[]>("getVideoDevices")
        ?? ValueTask.FromResult(Array.Empty<Device>());

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.StartVideo" />
    public void StartVideo(
        string deviceId,
        string selector) =>
        _siteModule?.InvokeVoid(
            "startVideo", deviceId, selector);

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.CreateOrJoinRoom" />
    public bool CreateOrJoinRoom(
        string roomName,
        string token) =>
        _siteModule?.Invoke<bool>(
            "createOrJoinRoom", roomName, token)
        ?? false;

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.LeaveRoom" />
    public void LeaveRoom() =>
        _siteModule?.InvokeVoid("leaveRoom");
}
