using Blazing.Twilio.VideoJSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Blazing.Twilio.Video.Pages
{
    public class IndexPage : ComponentBase
    {
        const string DefaultDeviceId = "default-device-id";
        protected const string DivId = "cam-1";
        protected string Selector => $"#{DivId}";

        [Inject] protected ProtectedLocalStorage LocalStore { get; set; } = null!;
        [Inject] protected IJSRuntime? JsRuntime { get; set; }

        protected string Room { get; set; }
        protected Device[]? Devices { get; set; }
        protected CameraState State { get; set; }
        protected bool HasDevices => State == CameraState.FoundCameras;
        protected bool IsLoading => State == CameraState.LoadingCameras;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && LocalStore != null)
            {
                Devices = await VideoJS.GetVideoDevicesAsync(JsRuntime);
                var defaultDeviceId = await LocalStore.GetAsync<string>(DefaultDeviceId);
                if (!string.IsNullOrWhiteSpace(defaultDeviceId))
                {
                    await SelectCamera(defaultDeviceId, false);
                    StateHasChanged();
                }
            }
        }

        protected async ValueTask SelectCamera(string deviceId, bool persist = true)
        {
            if (persist && LocalStore != null)
            {
                await LocalStore.SetAsync(DefaultDeviceId, deviceId);
            }

            await VideoJS.StartVideoAsync(JsRuntime, deviceId, Selector);
        }

        protected async ValueTask TryAddRoom(object args)
        {
            if (string.IsNullOrWhiteSpace(Room))
            {
                return;
            }

            if (args is KeyboardEventArgs keyboard && keyboard.Key == "Enter")
            {
                await VideoJS.
            }

            await new ValueTask();
        }
    }
}
