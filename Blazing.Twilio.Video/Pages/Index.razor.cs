using Blazing.Twilio.VideoJSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.ProtectedBrowserStorage;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Collections.Generic;
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
        [Inject] protected NavigationManager NavigationManager { get; set; } = null!;

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
                .WithUrl(NavigationManager.ToAbsoluteUri("/notificationHub"))
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<string>("RoomAdded", room =>
            {
                Rooms.Add(room);
                StateHasChanged();
            });

            await _hubConnection.StartAsync();
        }

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
                    await _hubConnection.InvokeAsync("RoomAdded", RoomName);
                    _activeRoom = RoomName;
                    RoomName = null;
                }
            }
        }

        protected bool IsActiveRoom(string room) => _activeRoom == room;
    }
}
