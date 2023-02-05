﻿// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Blazing.Twilio.Video.Client.Services;
using Microsoft.AspNetCore.SignalR.Client;

namespace Blazing.Twilio.Video.Client.Components;

public sealed partial class RoomDialog
{
    bool _isLoading = true;
    string? _roomName;
    HashSet<RoomDetails> _rooms = new();
    MudListItem? _selectedRoom;

    [Inject]
    public required HttpClient Http { get; set; }

    [Inject]
    public required AppState AppState { get; set; }

    [CascadingParameter]
    public required MudDialogInstance MudDialog { get; set; }

    [CascadingParameter]
    public required AppEventSubject AppEvents { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback<(string RoomName, string TwilioToken)> RoomCreatedOrJoined { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _rooms = await Http.GetFromJsonAsync<HashSet<RoomDetails>>("api/twilio/rooms")
            ?? new();

        _isLoading = false;
    }

    void Ok() => MudDialog.Close(DialogResult.Ok(true));

    async ValueTask TryAddRoom(KeyboardEventArgs args)
    {
        if (_roomName.IsNullOrEmpty())
        {
            return;
        }

        if (args.Key is "Enter")
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

        //AppEvents.TriggerAppEvent(new AppEventMessage(
        //    Value: ));

        if (RoomCreatedOrJoined.HasDelegate)
        {
            await RoomCreatedOrJoined.InvokeAsync((roomName!, jwt.Token));
        }

        return true;

        //var joined = JavaScript.CreateOrJoinRoom(roomName!, jwt.Token);
        //if (joined && _hubConnection is not null)
        //{
        //    AppState.ActiveRoomName = roomName;
        //    await _hubConnection.InvokeAsync(HubEventNames.RoomsUpdated, AppState.ActiveRoomName);
        //}
        //
        //return joined;
    }
}
