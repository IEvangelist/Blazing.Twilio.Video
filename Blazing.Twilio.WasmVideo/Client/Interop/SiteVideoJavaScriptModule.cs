// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.WasmVideo.Client.Interop;

/// <inheritdoc cref="ISiteVideoJavaScriptModule" />
internal sealed class SiteVideoJavaScriptModule : ISiteVideoJavaScriptModule
{
    private readonly IJSInProcessRuntime _javaScript;
    private IJSInProcessObjectReference? _siteModule;

    public SiteVideoJavaScriptModule(IJSInProcessRuntime javaScript)
    {
        ArgumentNullException.ThrowIfNull(javaScript);

        _javaScript = javaScript;
    }

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.InitiailizeModuleAsync" />
    public async ValueTask InitiailizeModuleAsync() =>
        _siteModule =
            await _javaScript.InvokeAsync<IJSInProcessObjectReference>(
                "import",
                "./site.js");

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.GetVideoDevicesAsync" />
    public ValueTask<Device[]> GetVideoDevicesAsync() =>
          _siteModule?.InvokeAsync<Device[]>(
              "getVideoDevices") ?? ValueTask.FromResult(Array.Empty<Device>());

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.StartVideoAsync" />
    public ValueTask StartVideoAsync(
        string deviceId,
        string selector) =>
        _siteModule?.InvokeVoidAsync(
            "startVideo",
            deviceId, selector) ?? ValueTask.CompletedTask;

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.CreateOrJoinRoomAsync" />
    public ValueTask<bool> CreateOrJoinRoomAsync(
        string roomName,
        string token) =>
        _siteModule?.InvokeAsync<bool>(
            "createOrJoinRoom",
            roomName, token) ?? ValueTask.FromResult(false);

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.LeaveRoomAsync" />
    public ValueTask LeaveRoomAsync() =>
        _siteModule?.InvokeVoidAsync(
            "leaveRoom") ?? ValueTask.CompletedTask;
}
