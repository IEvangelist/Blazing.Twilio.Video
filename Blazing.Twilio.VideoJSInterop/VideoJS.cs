using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Blazing.Twilio.VideoJSInterop
{
    public class VideoJS
    {
         /*
          * TODO: API surface to include:
          * 
          *     Creating or joining a room given a name
          *     Getting all rooms (display name and id)
          *     Get auth token from Web API
          *     SignalR: broadcast new rooms
          * 
          */

        public static ValueTask<Device[]> GetVideoDevicesAsync(
            IJSRuntime? jsRuntime) =>
            jsRuntime?.InvokeAsync<Device[]>(
                "videoInterop.getVideoDevices") ?? new ValueTask<Device[]>();

        public static ValueTask StartVideoAsync(
            IJSRuntime? jSRuntime,
            string deviceId,
            string selector) =>
            jSRuntime?.InvokeVoidAsync(
                "videoInterop.startVideo",
                deviceId, selector) ?? new ValueTask();
    }
}
