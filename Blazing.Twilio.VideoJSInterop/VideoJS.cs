using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Blazing.Twilio.VideoJSInterop
{
    public class VideoJS
    {
        public static ValueTask<Device[]> GetVideoDevicesAsync(
            IJSRuntime? jsRuntime) =>
            jsRuntime?.InvokeAsync<Device[]>(
                "videoInterop.getVideoDevices") ?? new ValueTask<Device[]>();

        public static ValueTask StartVideoAsync(
            IJSRuntime? jSRuntime,
            string deviceId,
            string containerId) =>
            jSRuntime?.InvokeVoidAsync(
                "videoInterop.startVideo",
                deviceId, containerId) ?? new ValueTask();
    }
}
