// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Interop;

/// <inheritdoc cref="ISiteVideoJavaScriptModule" />
internal sealed class SiteVideoJavaScriptModule : ISiteVideoJavaScriptModule
{
    readonly string _cacheBust = Guid.NewGuid().ToString();
    readonly IJSInProcessRuntime _javaScript;
    readonly ILogger<SiteVideoJavaScriptModule> _logger;
    readonly AppState _appState;
    readonly SemaphoreSlim _lock = new(1, 1);

    IJSInProcessObjectReference? _siteModule;

    public SiteVideoJavaScriptModule(
        IJSInProcessRuntime javaScript,
        ILogger<SiteVideoJavaScriptModule> logger,
        AppState appState)
    {
        ArgumentNullException.ThrowIfNull(_javaScript = javaScript);
        ArgumentNullException.ThrowIfNull(_logger = logger);
        ArgumentNullException.ThrowIfNull(_appState = appState);
    }

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.InitializeModuleAsync" />
    public async ValueTask InitializeModuleAsync()
    {
        var uninitialized = _siteModule is null;

        _siteModule ??=
            await _javaScript.InvokeAsync<IJSInProcessObjectReference>(
                "import", $"./site.js?{_cacheBust}");

        var emoji = uninitialized ? "1️⃣" : "2️⃣";

        _logger.LogInformation(
            "{Emoji} Initialized site.js (1st call: {Uninitialized})",
            emoji, uninitialized);
    }

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.GetVideoDevicesAsync" />
    public ValueTask<Device[]> GetVideoDevicesAsync() =>
        _siteModule?.InvokeAsync<Device[]>("getVideoDevices")
        ?? ValueTask.FromResult(Array.Empty<Device>());

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.StartVideoAsync" />
    public async ValueTask<bool> StartVideoAsync(
        string? deviceId,
        string? selector,
        CancellationToken token)
    {
        _logger.LogInformation("🤓 Requesting exclusive start video lock...");
        await _lock.WaitAsync(token);
        _logger.LogInformation("🎯 Acquired: (and temporarily using an exclusive 'start video' lock).");

        try
        {
            var availableToAttempt = _appState is { CameraStatus: CameraStatus.Idle }
                or { CameraStatus: CameraStatus.RequestingPreview }
                && deviceId is not null && selector is not null;

            if (availableToAttempt)
            {
                var (videoStarted, errorMessage) =
                    await (_siteModule?.InvokeAsync<(bool, string?)>(
                    "startVideo", token, deviceId, selector)
                    ?? ValueTask.FromResult<(bool, string?)>((false, null)));

                if (videoStarted)
                {
                    _logger.LogInformation(
                        "Video started (using 📹 ID: {DeviceId}) [target='{Selector}'].", deviceId, selector);
                    _appState.CameraStatus = selector switch
                    {
                        ElementIds.CameraPreview => CameraStatus.Previewing,
                        ElementIds.ParticipantOne or ElementIds.ParticipantTwo => CameraStatus.InCall,
                        _ => CameraStatus.Idle
                    };
                    return true;
                }
                else
                {
                    _logger.LogInformation(
                        "😥 Unable to start video. {Error}", errorMessage ?? "Unknown JS error.");
                    return false;
                }
            }
            else
            {
                _logger.LogInformation(
                    "🤖 Unable to start video (camera state: {State}).", _appState.CameraStatus);
                return false;
            }
        }
        finally
        {
            _logger.LogInformation("🔐 Releasing exclusive 'start video' lock.");
            _lock.Release();
            _logger.LogInformation("🔓 Released lock (camera state: {State}).", _appState.CameraStatus);
        }
    }

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.StopVideo" />
    public void StopVideo()
    {
        if (_appState is { CameraStatus: CameraStatus.Idle })
        {
            _logger.LogInformation("Unable to stop video as it's not currently running (Idle).");
            return;
        }

        _siteModule?.InvokeVoid("stopVideo");
        _logger.LogInformation("‼️ Stopped video... {State}", _appState.CameraStatus);
    }

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.CreateOrJoinRoomAsync" />
    public async ValueTask<bool> CreateOrJoinRoomAsync(
        string roomName,
        string token)
    {
        var cameraIsNotIdle = _appState is not { CameraStatus: CameraStatus.Idle };
        if (cameraIsNotIdle)
        {
            _logger.LogInformation(
                "📹 Camera is not idle, it's {Status}.", _appState.CameraStatus);
        }

        var createdOrJoinedRoom = cameraIsNotIdle
            && await
            (
                _siteModule?.InvokeAsync<bool>("createOrJoinRoom", roomName, token)
                ?? ValueTask.FromResult(false)
            );

        _logger.LogInformation(
            """✅ Created or joined room "{Room}": {Val}.""", roomName, createdOrJoinedRoom);

        _appState.CameraStatus =
            createdOrJoinedRoom ? CameraStatus.InCall : CameraStatus.Previewing;

        return createdOrJoinedRoom;
    }

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.LeaveRoom" />
    public bool LeaveRoom()
    {
        if (_appState is { CameraStatus: CameraStatus.Idle })
        {
            _logger.LogInformation("⁉️ Unable to leave room (already idle).");
            return false;
        }

        _logger.LogInformation("🤗 Leaving room (camera state: {State}).", _appState.CameraStatus);
        var left = _siteModule?.Invoke<bool>("leaveRoom") ?? false;
        _logger.LogInformation(
            "👋🏽 Left={Val} room (camera state: {State}).", left, _appState.CameraStatus);

        return left;
    }

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.LogInfo(string, object?[])" />
    public void LogInfo(string message, params object?[] args) =>
        _siteModule?.InvokeVoid("log", message, args);
}
