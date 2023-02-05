﻿// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Components;

public sealed partial class CameraDialog : IDisposable
{
    [Inject]
    public required ISiteVideoJavaScriptModule JavaScript { get; set; }

    [Inject]
    public required AppState AppState { get; set; }

    [CascadingParameter]
    public required MudDialogInstance MudDialog { get; set; }

    [EditorRequired, Parameter]
    public EventCallback<string> CameraChanged { get; set; }

    Device[]? Devices { get; set; }
    RequestCameraState State { get; set; }
    bool CameraDevicesFound => State is RequestCameraState.FoundCameras;
    bool IsRequesting => State is RequestCameraState.RequestingCameras;

    string? _selectedCameraId;

    protected override async Task OnInitializedAsync()
    {
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

    void OnValueChanged(string selectedValue)
    {
        _selectedCameraId = selectedValue;
        JavaScript.StartVideo(
            _selectedCameraId, "#camera-preview");
    }

    async Task SaveCameraSelection()
    {
        AppState.SelectedCameraId = _selectedCameraId;

        if (CameraChanged.HasDelegate)
        {
            await CameraChanged.InvokeAsync(_selectedCameraId);
        }

        MudDialog.Close(DialogResult.Ok(true));
        JavaScript.StopVideo();
    }

    void Cancel()
    {
        MudDialog.Cancel();
        JavaScript.StopVideo();
    }

    void IDisposable.Dispose() => JavaScript.StopVideo();
}
