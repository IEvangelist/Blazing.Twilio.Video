﻿// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Pages;

public sealed partial class Index : IDisposable
{
    [Inject]
    public required AppState AppState { get; set; }

    [Inject]
    public required ILogger<Index> Logger { get; set; }

    [CascadingParameter]
    public required AppEventSubject AppEvents { get; set; }
    
    protected override void OnInitialized()
    {
        AppState.StateChanged += OnStateHasChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        //if (AppState.SelectedCameraId is { } deviceId)
        //{
        //    var selector = AppState is { CameraStatus: CameraStatus.RequestingPreview }
        //        ? ElementIds.CameraPreview
        //        : ElementIds.ParticipantOne;
        //
        //    await SiteJavaScriptModule.StartVideoAsync(deviceId, selector);
        //}
    }

    void OnStateHasChanged(string appStatePropertyName)
    {
        var value = appStatePropertyName switch
        {
            nameof(AppState.SelectedCameraId) => AppState.SelectedCameraId,
            nameof(AppState.CameraStatus) => AppState.CameraStatus.ToString(),
            nameof(AppState.Rooms) => $"{AppState.Rooms?.Count ?? 0} (room count)",
            nameof(AppState.IsDarkTheme) => AppState.IsDarkTheme.ToString(),
            nameof(AppState.ActiveRoomName) => AppState.ActiveRoomName,
            _ => "Unknown"
        };

        Logger.LogInformation("⚙️ Changed: AppState.{AppStatePropertyName} = {Value}",
            appStatePropertyName, value);
    }

    void IDisposable.Dispose() => AppState.StateChanged -= OnStateHasChanged;
}