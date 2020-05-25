using Blazing.Twilio.Video.Hubs;
using Blazing.Twilio.VideoJSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.ProtectedBrowserStorage;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blazing.Twilio.Video.Pages
{
    public class IndexPage : ComponentBase
    {
        [Inject] protected IJSRuntime? JsRuntime { get; set; }
        [Inject] protected ProtectedLocalStorage LocalStore { get; set; } = null!;
        [Inject] protected NavigationManager NavigationManager { get; set; } = null!;

        const string DefaultDeviceId = "default-device-id";

        protected const string CameraElementId = "cam-1";
        protected string Selector => $"#{CameraElementId}";

        protected List<string> Rooms { get; set; } = new List<string>();
        protected string? RoomName { get; set; }
        protected Device[]? Devices { get; set; }
        protected CameraState State { get; set; }
        protected bool HasDevices => State == CameraState.FoundCameras;
        protected bool IsLoading => State == CameraState.LoadingCameras;

        HubConnection? _hubConnection;
        string? _activeRoom;

        protected override async Task OnInitializedAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                .AddMessagePackProtocol()
                .WithUrl(NavigationManager.ToAbsoluteUri(HubEndpoints.Notifications))
                .WithAutomaticReconnect()
                .ConfigureLogging(builder => builder.AddDebug().AddConsole())
                .Build();

            _hubConnection.On<string>("RoomAdded", async room =>
            {
                Rooms.Add(room);
                await InvokeAsync(() => StateHasChanged());
            });

            await _hubConnection.StartAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
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
            if (persist)
            {
                await LocalStore.SetAsync(DefaultDeviceId, deviceId);
            }

            await VideoJS.StartVideoAsync(JsRuntime, deviceId, Selector);
        }

        protected async ValueTask TryAddRoom(object args)
        {
            if (string.IsNullOrWhiteSpace(RoomName))
            {
                return;
            }

            var takeAction = args switch
            {
                KeyboardEventArgs keyboard when keyboard.Key == "Enter" => true,
                MouseEventArgs _ => true,
                _ => false
            };

            if (takeAction)
            {
                var addedOrJoined = await VideoJS.CreateOrJoinRoomAsync(JsRuntime, RoomName);
                if (addedOrJoined)
                {
                    await _hubConnection.InvokeAsync(HubEndpoints.RoomAdded, RoomName);
                    _activeRoom = RoomName;
                    RoomName = null;
                }
            }
        }

        protected bool IsActiveRoom(string room) => _activeRoom == room;
    }
}
