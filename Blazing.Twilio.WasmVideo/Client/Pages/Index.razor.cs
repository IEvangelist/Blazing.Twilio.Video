using Blazing.Twilio.WasmVideo.Client.Interop;
using Blazing.Twilio.WasmVideo.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Blazing.Twilio.WasmVideo.Client.Pages
{
    public partial class Index
    {
        [Inject] 
        protected IJSRuntime? JsRuntime { get; set; }
        [Inject] 
        protected NavigationManager NavigationManager { get; set; } = null!;
        [Inject]
        protected HttpClient Http { get; set; } = null!;

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
                .WithUrl(NavigationManager.ToAbsoluteUri(HubEndpoints.NotificationHub))
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<string>(HubEndpoints.RoomAdded, OnRoomAdded);

            await _hubConnection.StartAsync();

            Devices = await VideoJS.GetVideoDevicesAsync(JsRuntime);
            State = Devices != null && Devices.Length > 0
                    ? CameraState.FoundCameras
                    : CameraState.Error;

            StateHasChanged();
        }

        async Task OnRoomAdded(string roomName) =>
            await InvokeAsync(() =>
            {
                Rooms.Add(roomName);
                StateHasChanged();
            });

        protected async ValueTask SelectCamera(string deviceId) =>
            await VideoJS.StartVideoAsync(JsRuntime, deviceId, Selector);

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
                var jwt = await Http.GetFromJsonAsync<TwilioJwt>("api/twilio/token");
                if (jwt?.Token is null)
                {
                    return;
                }

                var addedOrJoined = await VideoJS.CreateOrJoinRoomAsync(JsRuntime, RoomName, jwt.Token);
                if (addedOrJoined)
                {
                    _activeRoom = RoomName;
                    RoomName = null;

                    await _hubConnection.InvokeAsync(HubEndpoints.RoomAdded, _activeRoom);
                }
            }
        }

        protected async ValueTask TryJoinRoom(string roomName)
        {
            if (string.IsNullOrWhiteSpace(roomName))
            {
                return;
            }

            var jwt = await Http.GetFromJsonAsync<TwilioJwt>("api/twilio/token");
            if (jwt?.Token is null)
            {
                return;
            }

            var joined = await VideoJS.CreateOrJoinRoomAsync(JsRuntime, roomName, jwt.Token);
            if (joined)
            {
                _activeRoom = roomName;
            }
        }

        protected bool IsActiveRoom(string room) => _activeRoom == room;
    }
}