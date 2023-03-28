// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Components;

public sealed partial class RoomDialog
{
    bool _isLoading = true;
    string? _roomName;
    MudListItem? _selectedRoom;

    [Inject] public required HttpClient Http { get; set; }

    [Inject] public required AppState AppState { get; set; }

    [Inject] public required ILogger<RoomDialog> Logger { get; set; }

    [Parameter] public required AppEventSubject AppEvents { get; set; }

    [CascadingParameter] public required MudDialogInstance MudDialog { get; set; }

    protected override async Task OnInitializedAsync()
    {
        AppState.Rooms = await Http.GetFromJsonAsync<HashSet<RoomDetails>>("api/twilio/rooms")
            ?? new();

        _isLoading = false;
    }

    void Ok() => MudDialog.Close(DialogResult.Ok(true));

    async ValueTask TryAddRoom(object args)
    {
        if (_roomName.IsNullOrEmpty())
        {
            return;
        }

        var takeAction = args switch
        {
            KeyboardEventArgs keyboard
            when keyboard.Key.Equals(
                "Enter", StringComparison.OrdinalIgnoreCase) => true,

            MouseEventArgs => true,
            _ => false
        };

        if (takeAction)
        {
            var addedOrJoined = await TryJoinRoom(_roomName);
            if (addedOrJoined)
            {
                Logger.LogInformation("Added or joined {room}", _roomName);
                _roomName = null;

                // Let the UI update enough to let the user know the
                // room was created/joined, before closing the modal.
                await Task.Delay(300);

                Ok();
            }
            else
            {
                Logger.LogInformation("Unable to add or join {room}", _roomName);
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

        AppEvents.TriggerAppEvent(new AppEventMessage(
            Value: roomName,
            TwilioToken: jwt.Token,
            MessageType: MessageType.CreateOrJoinRoom));

        return true;
    }
}
