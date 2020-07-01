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
        protected IJSRuntime? JavaScript { get; set; }
        [Inject] 
        protected NavigationManager NavigationManager { get; set; } = null!;
        [Inject]
        protected HttpClient Http { get; set; } = null!;

        protected List<RoomDetails> Rooms { get; private set; } = new List<RoomDetails>();

        string? _roomName;
        string? _activeCamera;
        string? _activeRoom;
        HubConnection? _hubConnection;

        protected override async Task OnInitializedAsync()
        {
            Rooms = await Http.GetFromJsonAsync<List<RoomDetails>>("api/twilio/rooms");

            _hubConnection = new HubConnectionBuilder()
                .AddMessagePackProtocol()
                .WithUrl(NavigationManager.ToAbsoluteUri(HubEndpoints.NotificationHub))
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<string>(HubEndpoints.RoomsUpdated, OnRoomAdded);

            await _hubConnection.StartAsync();
        }

        async ValueTask OnLeaveRoom()
        {
            await VideoJS.LeaveRoomAsync(JavaScript);
            await _hubConnection.InvokeAsync(HubEndpoints.RoomsUpdated, _activeRoom = null);
            if (!string.IsNullOrWhiteSpace(_activeCamera))
            {
                await VideoJS.StartVideoAsync(JavaScript, _activeCamera, "#camera");
            }
        }

        async Task OnCameraChanged(string activeCamera) => 
            await InvokeAsync(() => _activeCamera = activeCamera);

        async Task OnRoomAdded(string roomName) =>
            await InvokeAsync(async () =>
            {
                Rooms = await Http.GetFromJsonAsync<List<RoomDetails>>("api/twilio/rooms");
                StateHasChanged();
            });

        protected async ValueTask TryAddRoom(object args)
        {
            if (string.IsNullOrWhiteSpace(_roomName))
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
                var addedOrJoined = await TryJoinRoom(_roomName);
                if (addedOrJoined)
                {
                    _roomName = null;
                }
            }
        }

        protected async ValueTask<bool> TryJoinRoom(string? roomName)
        {
            if (string.IsNullOrWhiteSpace(roomName))
            {
                return false;
            }

            var jwt = await Http.GetFromJsonAsync<TwilioJwt>("api/twilio/token");
            if (jwt?.Token is null)
            {
                return false;
            }

            var joined = await VideoJS.CreateOrJoinRoomAsync(JavaScript, roomName, jwt.Token);
            if (joined)
            {
                _activeRoom = roomName;
                await _hubConnection.InvokeAsync(HubEndpoints.RoomsUpdated, _activeRoom);
            }

            return joined;
        }
    }
}