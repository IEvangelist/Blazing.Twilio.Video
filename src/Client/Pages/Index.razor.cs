// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Blazing.Twilio.Video.Client.Services;

namespace Blazing.Twilio.Video.Client.Pages;

public sealed partial class Index : IDisposable
{
    [Inject]
    public required ISiteVideoJavaScriptModule JavaScript { get; set; }

    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    [Inject]
    public required HttpClient Http { get; set; }

    [Inject]
    public required AppState AppState { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    [CascadingParameter]
    public required AppEventSubject AppEvents { get; set; }

    RoomDetails _selectedRoom;
    string? _roomName;
    HubConnection? _hubConnection;

    protected override async Task OnInitializedAsync()
    {
        AppState.StateChanged += StateHasChanged;

        await JavaScript.InitializeModuleAsync();

        _hubConnection = new HubConnectionBuilder()
            .AddMessagePackProtocol()
            .WithUrl(NavigationManager.ToAbsoluteUri(HubEndpoints.NotificationHub))
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<string>(HubEventNames.RoomsUpdated, OnRoomAdded);
        _hubConnection.On<string>(HubEventNames.UserConnected, OnUserConnected);

        await _hubConnection.StartAsync();
    }

    async ValueTask OnLeaveRoom()
    {
        if (_hubConnection is null)
            return;

        JavaScript.LeaveRoom();

        await Task.CompletedTask;
        //await _hubConnection.InvokeAsync(HubEventNames.RoomsUpdated, _activeRoom = null);
        //if (!ActiveCamera.IsNullOrWhiteSpace())
        //{
        //    JavaScript.StartVideo(ActiveCamera, "#camera");
        //}
    }

    Task OnRoomAdded(string roomName) =>
        InvokeAsync(async () =>
        {
            AppState.Rooms = await Http.GetFromJsonAsync<HashSet<RoomDetails>>("api/twilio/rooms")
                ?? new();

            Snackbar.Add(
                $"🆕 {roomName} was just created.",
                Severity.Info,
                options => options.CloseAfterNavigation = true);
        });

    Task OnUserConnected(string message) =>
        InvokeAsync(() =>
            Snackbar.Add(
                message,
                Severity.Info,
                options => options.CloseAfterNavigation = true));

    async ValueTask TryAddRoom(object args)
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

    async ValueTask<bool> TryJoinRoom(string? roomName)
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

        var joined = JavaScript.CreateOrJoinRoom(roomName!, jwt.Token);
        if (joined && _hubConnection is not null)
        {
            AppState.ActiveRoomName = roomName;
            await _hubConnection.InvokeAsync(HubEventNames.RoomsUpdated, AppState.ActiveRoomName);
        }

        return joined;
    }

    void IDisposable.Dispose() => AppState.StateChanged -= StateHasChanged;
}