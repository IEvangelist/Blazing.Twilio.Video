// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Components;

public sealed partial class CameraDialog : IDisposable
{
    [Inject] public required ISiteVideoJavaScriptModule JavaScript { get; set; }

    [Inject] public required AppState AppState { get; set; }

    [Inject] public required ILogger<CameraDialog> Logger { get; set; }

    [Parameter]
    public required AppEventSubject AppEvents { get; set; }

    [CascadingParameter]
    public required MudDialogInstance MudDialog { get; set; }

    Device[]? Devices { get; set; }
    RequestCameraState State { get; set; }
    bool CameraDevicesFound => State is RequestCameraState.FoundCameras;
    bool IsRequesting => State is RequestCameraState.RequestingCameras;

    string? _selectedCameraId;

    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("Initializing...");

        AppState.CameraStatus = CameraStatus.RequestingPreview;
        Devices = await JavaScript.GetVideoDevicesAsync();
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
    }

    async Task OnValueChanged(string selectedValue)
    {
        Logger.LogInformation("Camera selected...{Id}", selectedValue);

        _selectedCameraId = selectedValue;

        await JavaScript.StartVideoAsync(
            _selectedCameraId, ElementIds.CameraPreview);
    }

    void SaveCameraSelection()
    {
        MudDialog.Close(DialogResult.Ok(true));
        AppEvents.TriggerAppEvent(new AppEventMessage(
            Value: _selectedCameraId!,
            MessageType: MessageType.CameraSelected));
    }

    void Cancel()
    {
        MudDialog.Cancel();
        JavaScript.StopVideo();
    }

    void IDisposable.Dispose() => JavaScript.StopVideo();
}
