// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Pages;

public sealed partial class Index : IDisposable
{
    [Inject]
    public required ISiteVideoJavaScriptModule JavaScript { get; set; }

    [Inject]
    public required AppState AppState { get; set; }

    [CascadingParameter]
    public required AppEventSubject AppEvents { get; set; }

    protected override async Task OnInitializedAsync()
    {
        AppState.StateChanged += OnStateHasChanged;

        await JavaScript.InitializeModuleAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (AppState.SelectedCameraId is { } deviceId)
        {
            var selector = AppState is { CameraStatus: CameraStatus.RequestingPreview }
                ? ElementIds.CameraPreview
                : ElementIds.ParticipantOne;

            await JavaScript.StartVideoAsync(deviceId, selector);
        }
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

        Console.WriteLine(
            $"Changed: AppState.{appStatePropertyName} = {value}");
    }

    void IDisposable.Dispose() => AppState.StateChanged -= OnStateHasChanged;
}