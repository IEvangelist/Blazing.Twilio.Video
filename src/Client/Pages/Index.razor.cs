// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Pages;

public sealed partial class Index : IDisposable
{
    /// <summary>The calculated classes for the <c>video</c> HTML elements.</summary>
    string MidCameraClass =>
        HideHeader ? "min-vh-85" : "min-vh-55";

    /// <summary>Hide the header when either single size column is not maximized.</summary>
    bool HideHeader => OneSize is 10 || TwoSize is 10;

    /// <summary>The "#participant-1 > video" HTML element.</summary>
    int OneSize =>
        AppState switch
        {
            { CameraStatus: CameraStatus.PictureInPicture } => 0,
            {
                CameraStatus:
                    CameraStatus.Idle or
                    CameraStatus.InCall or
                    CameraStatus.RequestingPreview or
                    CameraStatus.Previewing
            } => 5,
            _ => 10
        };

    /// <summary>Is <c>"hidden"</c> when <see cref="OneSize"/> is <c>0</c>.
    /// Otherwise, <c>"min-vh-55"</c> is returned.</summary>
    string OneClass => OneSize switch
    {
        0 => "hidden",
        _ => "min-vh-55"
    };

    /// <summary>The "#participant-2 > video" HTML element.</summary>
    int TwoSize =>
        AppState switch
        {
            { CameraStatus: CameraStatus.PictureInPicture } => 10,
            { CameraStatus:
                CameraStatus.Idle or
                CameraStatus.InCall or
                CameraStatus.RequestingPreview or
                CameraStatus.Previewing
            } => 5,
            _ => 0
        };

    /// <summary>Is <c>"hidden"</c> when <see cref="TwoSize"/> is <c>0</c>.
    /// Otherwise, <c>"min-vh-55"</c> is returned.</summary>
    string TwoClass => TwoSize switch
    {
        0 => "hidden",
        _ => "min-vh-55"
    };

    [Inject] public required AppState AppState { get; set; }

    [Inject] public required ILogger<Index> Logger { get; set; }

    [CascadingParameter]
    public required AppEventSubject AppEvents { get; set; }
    
    protected override void OnInitialized()
    {
        AppState.StateChanged += OnStateHasChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender is false && AppState.SelectedCameraId is { } deviceId)
        {
            if (AppState is { CameraStatus: CameraStatus.PictureInPicture or CameraStatus.InCall })
            {
                return;
            }

            var selector = AppState is { CameraStatus: CameraStatus.RequestingPreview }
                ? ElementIds.CameraPreview
                : ElementIds.ParticipantOne;

            await SiteJavaScriptModule.StartVideoAsync(deviceId, selector);
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

        Logger.LogInformation("⚙️ Changed: AppState.{AppStatePropertyName} = {Value}",
            appStatePropertyName, value);
    }

    void IDisposable.Dispose() => AppState.StateChanged -= OnStateHasChanged;
}