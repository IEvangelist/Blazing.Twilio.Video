using Blazing.Twilio.VideoJSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Linq;
using System.Threading.Tasks;

namespace Blazing.Twilio.Video.Pages
{
    public class IndexPage : ComponentBase
    {
        [Inject]
        protected IJSRuntime? JsRuntime { get; set; }

        protected string CameraContainerId = "camera-container";
        protected Device[]? Devices { get; set; }
        protected CameraState State { get; set; }
        protected bool HasDevices => State == CameraState.FoundCameras;
        protected bool IsLoading => State == CameraState.LoadingCameras;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Devices = await VideoJS.GetVideoDevicesAsync(JsRuntime);
                State = Devices?.Any() ?? false
                    ? CameraState.FoundCameras
                    : CameraState.Error;
                StateHasChanged();
            }
        }

        protected ValueTask SelectCamera(string deviceId) =>
            VideoJS.StartVideoAsync(JsRuntime, deviceId, CameraContainerId);

        protected enum CameraState
        {
            LoadingCameras,
            FoundCameras,
            Error
        }
    }
}
