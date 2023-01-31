// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Pages;

public partial class Index
{
    [Inject]
    public required ISiteVideoJavaScriptModule JavaScript { get; set; }
    [Inject]
    public required NavigationManager NavigationManager { get; set; }
    [Inject]
    public required HttpClient Http { get; set; }
    [CascadingParameter]
    protected string? ActiveCamera { get; set; }

    List<RoomDetails> _rooms = new();

    string? _roomName;
    string? _activeRoom;
    HubConnection? _hubConnection;

    protected override async Task OnInitializedAsync()
    {
        await JavaScript.InitiailizeModuleAsync();

        _rooms = await Http.GetFromJsonAsync<List<RoomDetails>>("api/twilio/rooms")
            ?? new();

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
        if (_hubConnection is null)
            return;

        await JavaScript.LeaveRoomAsync();
        await _hubConnection.InvokeAsync(HubEndpoints.RoomsUpdated, _activeRoom = null);
        if (!ActiveCamera.IsNullOrWhiteSpace())
        {
            await JavaScript.StartVideoAsync(ActiveCamera, "#camera");
        }
    }

    Task OnRoomAdded(string roomName) =>
        InvokeAsync(async () =>
        {
            _rooms = await Http.GetFromJsonAsync<List<RoomDetails>>("api/twilio/rooms")
                ?? new();
            StateHasChanged();
        });

    protected async ValueTask TryAddRoom(object args)
    {
        if (_roomName.IsNullOrEmpty())
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
        if (roomName.IsNullOrWhiteSpace())
        {
            return false;
        }

        var jwt = await Http.GetFromJsonAsync<TwilioJwt>("api/twilio/token");
        if (jwt is { Token: null })
        {
            return false;
        }

        var joined = await JavaScript.CreateOrJoinRoomAsync(roomName!, jwt.Token);
        if (joined && _hubConnection is not null)
        {
            _activeRoom = roomName;
            await _hubConnection.InvokeAsync(HubEndpoints.RoomsUpdated, _activeRoom);
        }

        return joined;
    }
}