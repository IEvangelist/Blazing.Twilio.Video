// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using Blazing.Twilio.Video.Client.Models;
using Blazor.Serialization.Extensions;

namespace Blazing.Twilio.Video.Client.Components;

public sealed partial class CameraDialog : IDisposable
{
    [Inject] public required AppState AppState { get; set; }

    [Inject] public required ILogger<CameraDialog> Logger { get; set; }

    [Parameter] public required AppEventSubject AppEvents { get; set; }

    [CascadingParameter] public required MudDialogInstance MudDialog { get; set; }

    Device[]? Devices { get; set; }
    RequestCameraState State { get; set; }
    bool CameraDevicesFound => State is RequestCameraState.FoundCameras;
    bool IsRequesting => State is RequestCameraState.RequestingCameras;

    string? _selectedCameraId;

    protected override async Task OnInitializedAsync()
    {
        AppState.CameraStatus = CameraStatus.RequestingPreview;
        var json = await SiteJavaScriptModule.RequestVideoDevicesAsync();
        Logger.LogInformation("🎥 Devices: {Json}", json);
        Devices = json.FromJson<Device[]>(
            new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? Array.Empty<Device>();
        State = Devices switch
        {
            null or { Length: 0 } => RequestCameraState.Error,
            _ => RequestCameraState.FoundCameras
        };

        var selectedDeviceId = AppState.SelectedCameraId;
        if (!selectedDeviceId.IsNullOrWhiteSpace() && Devices is not null)
        {
            _selectedCameraId = selectedDeviceId;
        }

        await SiteJavaScriptModule.ExitPictureInPictureAsync(
            onExited: exited =>
            {
                if (exited)
                {
                    AppState.CameraStatus = CameraStatus.RequestingPreview;
                }
                else Logger.LogInformation(
                    "❔ Unable to evaluate (PiP) {Exited} as `true`.", exited);
            });
    }

    async Task OnValueChanged(string selectedValue)
    {
        Logger.LogInformation("Camera selected...{Id}", selectedValue);

        _selectedCameraId = selectedValue;

        if (await SiteJavaScriptModule.StartVideoAsync(
            _selectedCameraId, ElementSelectors.CameraPreviewId))
        {
            AppState.CameraStatus = CameraStatus.Previewing;
        }
    }

    void SaveCameraSelection()
    {
        SiteJavaScriptModule.StopVideo();
        AppState.CameraStatus = CameraStatus.Idle;
        MudDialog.Close(DialogResult.Ok(true));
        AppEvents.TriggerAppEvent(new AppEventMessage(
            Value: _selectedCameraId!,
            MessageType: MessageType.SelectCamera));
    }

    void Cancel()
    {
        MudDialog.Cancel();
        SiteJavaScriptModule.StopVideo();
    }

    void IDisposable.Dispose() => SiteJavaScriptModule.StopVideo();
}
