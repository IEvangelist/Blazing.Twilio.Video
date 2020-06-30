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
        protected IJSRuntime? JsRuntime { get; set; }
        protected Device[]? Devices { get; private set; }
        protected CameraState State { get; private set; }
        protected bool HasDevices => State == CameraState.FoundCameras;
        protected bool IsLoading => State == CameraState.LoadingCameras;

        [Parameter]
        public EventCallback<string> CameraChanged { get; set; }

        string? _activeCamera;

        protected override async Task OnInitializedAsync()
        {
            Devices = await VideoJS.GetVideoDevicesAsync(JsRuntime);
            State = Devices != null && Devices.Length > 0
                    ? CameraState.FoundCameras
                    : CameraState.Error;
        }

        protected async ValueTask SelectCamera(string deviceId)
        {
            await VideoJS.StartVideoAsync(JsRuntime, deviceId, "#camera");
            _activeCamera = deviceId;

            if (CameraChanged.HasDelegate)
            {
                await CameraChanged.InvokeAsync(_activeCamera);
            }
        }
    }
}
