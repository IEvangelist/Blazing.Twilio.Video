// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Shared;

public partial class MainLayout
{
    [Inject] public required NavigationManager NavigationManager { get; set; }
    [Inject] public required HttpClient Http { get; set; }
    [Inject] public required AppState AppState { get; set; }
    [Inject] public required IDialogService Dialog { get; set; }
    [Inject] public required ISnackbar Snackbar { get; set; }
    [Inject] public required ILogger<MainLayout> Logger { get; set; }

    string LeaveRoomQuestion => $"""
        Leave "{AppState.ActiveRoomName}" Room?
        """;

    AppEventSubject AppEvents { get; set; }

    HubConnection? _hubConnection;

    public MainLayout() =>
        AppEvents = new(OnAppEventMessageReceived);

    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("Initializing...");

        _hubConnection = new HubConnectionBuilder()
            .AddMessagePackProtocol()
            .WithUrl(NavigationManager.ToAbsoluteUri(HubEndpoints.NotificationHub))
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<string>(HubEventNames.RoomsUpdated, OnRoomAdded);
        _hubConnection.On<string>(HubEventNames.UserConnected, OnUserConnected);

        await _hubConnection.StartAsync();
    }

    void TryLeaveRoom()
    {
        if (SiteJavaScriptModule.LeaveRoom())
        {
            var roomName = AppState.ActiveRoomName;
            AppState.ActiveRoomName = null;
            Snackbar.Add(
                $"""You have left the "{roomName}" room.""",
                Severity.Info,
                options =>
                {
                    options.CloseAfterNavigation = true;
                    options.IconSize = Size.Large;
                });
        }
    }

    async Task OnAppEventMessageReceived(AppEventMessage eventMessage)
    {
        Logger.LogInformation(
            "App event message: {Type}", eventMessage.MessageType);

        if (eventMessage.MessageType is MessageType.LeftRoom)
        {
            TryLeaveRoom();
            return;
        }

        if (_hubConnection is not null &&
            eventMessage.MessageType is MessageType.RoomCreatedOrJoined)
        {
            await SiteJavaScriptModule.CreateOrJoinRoomAsync(
                eventMessage.Value, eventMessage.TwilioToken!);

            await _hubConnection.InvokeAsync(
                HubEventNames.RoomsUpdated,
                AppState.ActiveRoomName = eventMessage.Value);

            return;
        }

        if (eventMessage.MessageType is MessageType.CameraSelected &&
            (AppState.SelectedCameraId = eventMessage.Value) is { } deviceId)
        {
            if (await SiteJavaScriptModule.StartVideoAsync(deviceId, ElementIds.ParticipantOne))
            {
                AppState.CameraStatus = CameraStatus.InCall;
            }
        }
    }

    Task OnRoomAdded(string roomName) =>
        InvokeAsync(async () =>
        {
            AppState.Rooms = await Http.GetFromJsonAsync<HashSet<RoomDetails>>("api/twilio/rooms")
                ?? new();

            const string template = LogMessageTemplates.OnRoomAdded;

            Logger.LogInformation(template, roomName);
            Snackbar.Add(
                template.Replace("{RoomName}", roomName),
                Severity.Error,
                options =>
                {
                    options.CloseAfterNavigation = true;
                    options.IconSize = Size.Large;
                });
        });

    Task OnUserConnected(string message)
    {
        return InvokeAsync(() =>
        {
            const string template = LogMessageTemplates.OnUserConnected;

            Logger.LogInformation(LogMessageTemplates.OnUserConnected, message);
            Snackbar.Add(
                template.Replace("{Message}", message),
                Severity.Error,
                options =>
                {
                    options.CloseAfterNavigation = true;
                    options.IconSize = Size.Large;
                });
        });
    }

    Task OpenCameraDialog() =>
        ShowDialogAsync<CameraDialog>("Camera Preferences");

    Task OpenRoomDialog() =>
        ShowDialogAsync<RoomDialog>("Available Rooms");

    async Task ShowDialogAsync<TDialog>(string title) where TDialog : ComponentBase
    {
        var dialogReference =
            await Dialog.ShowAsync<TDialog>(title, new DialogParameters()
            {
                [nameof(AppEvents)] = AppEvents
            });

        if (await dialogReference.Result is { Canceled: false })
        {
            StateHasChanged();
        }
    }
}

file static class LogMessageTemplates
{
    /// <summary>
    /// If not passed as the <c>message</c> argument of
    /// the <see cref="ILogger{TCategoryName}.LogInformation"/> method,
    /// call <c>LogMessageTemplates.OnRoomAdded.Replace("{RoomName}", roomName);</c>
    /// </summary>
    internal const string OnRoomAdded = "🆕 {RoomName} was just created.";

    /// <summary>
    /// If not passed as the <c>message</c> argument of
    /// the <see cref="ILogger{TCategoryName}.LogInformation"/> method,
    /// call <c>LogMessageTemplates.OnUserConnected.Replace("{Message}", message);</c>
    /// </summary>
    internal const string OnUserConnected = "⚠️ {Message}";
}
