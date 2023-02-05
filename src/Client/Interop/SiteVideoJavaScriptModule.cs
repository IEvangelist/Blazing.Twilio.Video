// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Interop;

/// <inheritdoc cref="ISiteVideoJavaScriptModule" />
internal sealed class SiteVideoJavaScriptModule : ISiteVideoJavaScriptModule
{
    readonly string _queryString = Guid.NewGuid().ToString();
    readonly IJSInProcessRuntime _javaScript;

    IJSInProcessObjectReference? _siteModule;

    public SiteVideoJavaScriptModule(IJSInProcessRuntime javaScript) =>
        ArgumentNullException.ThrowIfNull(_javaScript = javaScript);

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.InitializeModuleAsync" />
    public async ValueTask InitializeModuleAsync() =>
        _siteModule ??=
            await _javaScript.InvokeAsync<IJSInProcessObjectReference>(
                "import", $"./site.js?{_queryString}");

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.GetVideoDevicesAsync" />
    public ValueTask<Device[]> GetVideoDevicesAsync() =>
        _siteModule?.InvokeAsync<Device[]>("getVideoDevices")
        ?? ValueTask.FromResult(Array.Empty<Device>());

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.StartVideo" />
    public void StartVideo(
        string? deviceId,
        string? selector)
    {
        if (deviceId is null || selector is null) return;
        _siteModule?.InvokeVoid("startVideo", deviceId, selector);
    }

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.StopVideo" />
    public void StopVideo() =>
        _siteModule?.InvokeVoid("stopVideo");

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.CreateOrJoinRoom" />
    public bool CreateOrJoinRoom(
        string roomName,
        string token) =>
        _siteModule?.Invoke<bool>("createOrJoinRoom", roomName, token) ?? false;

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.LeaveRoom" />
    public void LeaveRoom() =>
        _siteModule?.InvokeVoid("leaveRoom");
}
