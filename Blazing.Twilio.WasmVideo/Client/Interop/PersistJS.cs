using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Blazing.Twilio.WasmVideo.Client.Interop
{
    public static class PersistJS
    {
        public static ValueTask<T> GetAsync<T>(
              this IJSRuntime? jsRuntime,
              string key) =>
              jsRuntime?.InvokeAsync<T>(
                  "store.get", key) ?? new ValueTask<T>();

        public static ValueTask SetAsync(
              this IJSRuntime? jsRuntime,
              string key,
              string value) =>
              jsRuntime?.InvokeVoidAsync(
                  "store.set", key, value) ?? new ValueTask();

        public static ValueTask DeleteAsync(
              this IJSRuntime? jsRuntime,
              string key) =>
              jsRuntime?.InvokeVoidAsync(
                  "store.delete", key) ?? new ValueTask();
    }
}
