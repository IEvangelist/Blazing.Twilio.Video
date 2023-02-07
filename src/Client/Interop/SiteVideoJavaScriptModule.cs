// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Interop;

/// <inheritdoc cref="ISiteVideoJavaScriptModule" />
internal sealed class SiteVideoJavaScriptModule : ISiteVideoJavaScriptModule
{
    readonly string _cacheBust = Guid.NewGuid().ToString();
    readonly IJSInProcessRuntime _javaScript;
    readonly AppState _appState;

    IJSInProcessObjectReference? _siteModule;

    public SiteVideoJavaScriptModule(
        IJSInProcessRuntime javaScript,
        AppState appState)
    {
        ArgumentNullException.ThrowIfNull(_javaScript = javaScript);
        ArgumentNullException.ThrowIfNull(_appState = appState);
    }

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.InitializeModuleAsync" />
    public async ValueTask InitializeModuleAsync() =>
        _siteModule ??=
            await _javaScript.InvokeAsync<IJSInProcessObjectReference>(
                "import", $"./site.js?{_cacheBust}");

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.GetVideoDevicesAsync" />
    public ValueTask<Device[]> GetVideoDevicesAsync() =>
        _siteModule?.InvokeAsync<Device[]>("getVideoDevices")
        ?? ValueTask.FromResult(Array.Empty<Device>());

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.StartVideoAsync" />
    public async ValueTask<bool> StartVideoAsync(
        string? deviceId,
        string? selector) =>
        _appState is { CameraStatus: CameraStatus.Idle } &&
        deviceId is not null && selector is not null
            && await (_siteModule?.InvokeAsync<bool>("startVideo", deviceId, selector)
                ?? ValueTask.FromResult<bool>(false));

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.StopVideo" />
    public void StopVideo()
    {
        if (_appState is { CameraStatus: CameraStatus.Idle })
        {
            return;
        }
        
        _siteModule?.InvokeVoid("stopVideo");
    }

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.CreateOrJoinRoom" />
    public bool CreateOrJoinRoom(
        string roomName,
        string token) =>
        _appState is { CameraStatus: CameraStatus.Idle } &&
        (_siteModule?.Invoke<bool>("createOrJoinRoom", roomName, token) ?? false);

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.LeaveRoom" />
    public void LeaveRoom()
    {
        if (_appState is { CameraStatus: CameraStatus.Idle })
        {
            return;
        }
        
        _siteModule?.InvokeVoid("leaveRoom");
    }
}
