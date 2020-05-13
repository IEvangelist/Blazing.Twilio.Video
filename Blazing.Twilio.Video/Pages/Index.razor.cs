using Blazing.Twilio.VideoJSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Linq;
using System.Threading.Tasks;

namespace Blazing.Twilio.Video.Pages
{
    public class IndexPage : ComponentBase
    {
        [Inject]
        protected ProtectedLocalStorage LocalStore { get; set; }

        [Inject]
        protected IJSRuntime? JsRuntime { get; set; }

        protected const string CameraContainerId = "camera-container";
        protected const string DefaultDeviceId = "default-device-id";

        protected Device[]? Devices { get; set; }
        protected CameraState State { get; set; }
        protected bool HasDevices => State == CameraState.FoundCameras;
        protected bool IsLoading => State == CameraState.LoadingCameras;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && LocalStore != null)
            {
                Devices = await VideoJS.GetVideoDevicesAsync(JsRuntime);
                State = Devices?.Any() ?? false
                    ? CameraState.FoundCameras
                    : CameraState.Error;
                var defaultDeviceId = await LocalStore.GetAsync<string>(DefaultDeviceId);
                if (!string.IsNullOrWhiteSpace(defaultDeviceId))
                {
                    await SelectCamera(defaultDeviceId);
                }
                
                StateHasChanged();
            }
        }

        protected async ValueTask SelectCamera(string deviceId)
        {
            if (LocalStore != null)
            {
                await LocalStore.SetAsync(DefaultDeviceId, deviceId);
            }

            await VideoJS.StartVideoAsync(JsRuntime, deviceId, CameraContainerId);
        }

        protected enum CameraState
        {
            LoadingCameras,
            FoundCameras,
            Error
        }
    }
}
