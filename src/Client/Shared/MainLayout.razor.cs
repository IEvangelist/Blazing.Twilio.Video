// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Shared;

public partial class MainLayout
{
    [Inject] public required NavigationManager NavigationManager { get; set; }
    [Inject] public required HttpClient Http { get; set; }
    [Inject] public required AppState AppState { get; set; }
    [Inject] public required ISiteVideoJavaScriptModule JavaScript { get; set; }
    [Inject] public required IDialogService Dialog { get; set; }
    [Inject] public required ISnackbar Snackbar { get; set; }

    AppEventSubject AppEvents { get; set; }

    HubConnection? _hubConnection;

    public MainLayout() =>
        AppEvents = new(OnAppEventMessageReceived);

    protected override async Task OnInitializedAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .AddMessagePackProtocol()
            .WithUrl(NavigationManager.ToAbsoluteUri(HubEndpoints.NotificationHub))
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<string>(HubEventNames.RoomsUpdated, OnRoomAdded);
        _hubConnection.On<string>(HubEventNames.UserConnected, OnUserConnected);

        await _hubConnection.StartAsync();
    }

    async Task OnAppEventMessageReceived(AppEventMessage eventMessage)
    {
        if (eventMessage.MessageType is MessageType.LeftRoom)
        {
            JavaScript.LeaveRoom();
            return;
        }

        if (_hubConnection is not null &&
            eventMessage.MessageType is MessageType.RoomCreatedOrJoined)
        {
            await JavaScript.CreateOrJoinRoomAsync(
                eventMessage.Value, eventMessage.TwilioToken!);

            await _hubConnection.InvokeAsync(
                HubEventNames.RoomsUpdated,
                AppState.ActiveRoomName = eventMessage.Value);

            return;
        }

        if (eventMessage.MessageType is MessageType.CameraSelected &&
            (AppState.SelectedCameraId = eventMessage.Value) is { } deviceId)
        {
            var selector = AppState is { CameraStatus: CameraStatus.RequestingPreview }
                ? ElementIds.CameraPreview
                : ElementIds.ParticipantOne;

            await JavaScript.StartVideoAsync(deviceId, selector);
        }
    }

    Task OnRoomAdded(string roomName) =>
        InvokeAsync(async () =>
        {
            AppState.Rooms = await Http.GetFromJsonAsync<HashSet<RoomDetails>>("api/twilio/rooms")
                ?? new();

            Snackbar.Add(
                $"🆕 {roomName} was just created.",
                Severity.Error,
                options =>
                {
                    options.CloseAfterNavigation = true;
                    options.IconSize = Size.Large;
                });
        });

    Task OnUserConnected(string message) =>
        InvokeAsync(() =>
            Snackbar.Add(
                message,
                Severity.Error,
                options =>
                {
                    options.CloseAfterNavigation = true;
                    options.IconSize = Size.Large;
                }));

    Task OpenCameraDialog() =>
        ShowDialogAsync<CameraDialog>("Camera Preferences");

    Task OpenRoomDialog() =>
        ShowDialogAsync<RoomDialog>("Available Rooms");

    async Task ShowDialogAsync<TDialog>(string title) where TDialog : ComponentBase
    {
        var reference = await Dialog.ShowAsync<TDialog>(title, new DialogParameters()
        {
            [nameof(AppEvents)] = AppEvents
        });

        if (await reference.Result is { Canceled: false })
        {
            StateHasChanged();
        }
    }
}
