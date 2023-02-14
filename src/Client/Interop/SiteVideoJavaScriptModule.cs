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

        _logger.LogInformation("Initialized site.js (1st call: {First})", uninitialized);
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
        _logger.LogInformation("Acquiring exclusive start video lock.");
        await _lock.WaitAsync(token);

        try
        {
            var availableToAttempt = _appState is { CameraStatus: CameraStatus.Idle }
                && deviceId is not null && selector is not null;

            if (availableToAttempt)
            {
                var (videoStarted, errorMessage) =
                    await (_siteModule?.InvokeAsync<(bool, string?)>(
                    "startVideo", token, deviceId, selector)
                    ?? ValueTask.FromResult<(bool, string?)>((false, null)));

                if (videoStarted)
                {
                    _logger.LogInformation("Video started.");
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
                    _logger.LogInformation("Unable to start video. {Error}", errorMessage);
                    return false;
                }
            }
            else
            {
                _logger.LogInformation("Unable to start video.");
                return false;
            }
        }
        finally
        {
            _logger.LogInformation("Releasing exclusive start video lock.");
            _lock.Release();
        }
    }

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.StopVideo" />
    public void StopVideo()
    {
        if (_appState is { CameraStatus: CameraStatus.Idle })
        {
            _logger.LogInformation("Unable to stop video.");
            return;
        }

        _logger.LogInformation("Stopped video.");
        _siteModule?.InvokeVoid("stopVideo");
    }

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.CreateOrJoinRoomAsync" />
    public async ValueTask<bool> CreateOrJoinRoomAsync(
        string roomName,
        string token)
    {
        var createdOrJoinedRoom = _appState is not { CameraStatus: CameraStatus.Idle }
            && await (_siteModule?.InvokeAsync<bool>("createOrJoinRoom", roomName, token)
            ?? ValueTask.FromResult(false));

        _logger.LogInformation(
            "Created or joined room '{Room}': {Val}.", roomName, createdOrJoinedRoom);

        _appState.CameraStatus =
            createdOrJoinedRoom ? CameraStatus.InCall : CameraStatus.Previewing;

        return createdOrJoinedRoom;
    }

    /// <inheritdoc cref="ISiteVideoJavaScriptModule.LeaveRoom" />
    public void LeaveRoom()
    {
        if (_appState is { CameraStatus: CameraStatus.Idle })
        {
            _logger.LogInformation("Unable to leave room.");
            return;
        }

        _logger.LogInformation("Left room.");
        _siteModule?.InvokeVoid("leaveRoom");
    }
}
