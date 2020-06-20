using Blazing.Twilio.WasmVideo.Shared;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Blazing.Twilio.WasmVideo.Client.Interop
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
            string selector) =>
            jSRuntime?.InvokeVoidAsync(
                "videoInterop.startVideo",
                deviceId, selector) ?? new ValueTask();

        public static ValueTask<bool> CreateOrJoinRoomAsync(
            IJSRuntime? jsRuntime,
            string roomName,
            string token) =>
            jsRuntime?.InvokeAsync<bool>(
                "videoInterop.createOrJoinRoom",
                roomName, token) ?? new ValueTask<bool>(false);
    }
}
