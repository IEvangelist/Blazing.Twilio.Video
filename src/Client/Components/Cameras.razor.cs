// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Components;

public partial class Cameras
{
    [Inject]
    public required ISiteVideoJavaScriptModule JavaScript { get; set; }

    [Inject]
    public required ILocalStorageService LocalStorage { get; set; }

    [EditorRequired, Parameter]
    public EventCallback<string> CameraChanged { get; set; }

    const string DefaultDeviceId = "default-device-id";

    protected Device[]? Devices { get; private set; }
    protected CameraState State { get; private set; }
    protected bool HasDevices => State is CameraState.FoundCameras;
    protected bool IsLoading => State is CameraState.LoadingCameras;

    string? _activeCamera;

    protected override async Task OnInitializedAsync()
    {
        await JavaScript.InitiailizeModuleAsync();

        Devices = await JavaScript.GetVideoDevicesAsync();
        State = Devices != null && Devices.Length > 0
                ? CameraState.FoundCameras
                : CameraState.Error;

        var defaultDeviceId = LocalStorage.GetItem<string>(DefaultDeviceId);
        if (!defaultDeviceId.IsNullOrWhiteSpace())
        {
            await SelectCamera(defaultDeviceId, false);
        }
    }

    protected async ValueTask SelectCamera(string deviceId, bool persist = true)
    {
        if (persist)
        {
            LocalStorage.SetItem(DefaultDeviceId, deviceId);
        }

        JavaScript.StartVideo(
            _activeCamera = deviceId, "#camera");

        if (CameraChanged.HasDelegate)
        {
            await CameraChanged.InvokeAsync(_activeCamera);
        }
    }
}
