using Blazing.Twilio.WasmVideo.Client.Interop;
using Blazing.Twilio.WasmVideo.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Blazing.Twilio.WasmVideo.Client.Components
{
    public partial class Cameras
    {
        [Inject]
        protected IJSRuntime? JavaScript { get; set; }

        [Parameter]
        public EventCallback<string> CameraChanged { get; set; }

        const string DefaultDeviceId = "default-device-id";

        protected Device[]? Devices { get; private set; }
        protected CameraState State { get; private set; }
        protected bool HasDevices => State == CameraState.FoundCameras;
        protected bool IsLoading => State == CameraState.LoadingCameras;

        string? _activeCamera;

        protected override async Task OnInitializedAsync()
        {
            Devices = await JavaScript.GetVideoDevicesAsync();
            State = Devices != null && Devices.Length > 0
                    ? CameraState.FoundCameras
                    : CameraState.Error;

            var defaultDeviceId = await JavaScript.GetAsync<string>(DefaultDeviceId);
            if (!string.IsNullOrWhiteSpace(defaultDeviceId))
            {
                await SelectCamera(defaultDeviceId, false);
            }
        }

        protected async ValueTask SelectCamera(string deviceId, bool persist = true)
        {
            if (persist)
            {
                await JavaScript.SetAsync(DefaultDeviceId, deviceId);
            }

            await JavaScript.StartVideoAsync(deviceId, "#camera");
            _activeCamera = deviceId;

            if (CameraChanged.HasDelegate)
            {
                await CameraChanged.InvokeAsync(_activeCamera);
            }
        }
    }
}
