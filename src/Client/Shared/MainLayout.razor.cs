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

    readonly AppEventSubject _appEvents;

    HubConnection? _hubConnection;

    public MainLayout() =>
        _appEvents = new(OnAppEventMessageReceived);

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
            _ = SiteJavaScriptModule.ExitPictureInPictureAsync(exited =>
            {
                if (exited)
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
                else
                {
                    Logger.LogWarning("Unable to exit PiP mode.");
                }
            });
        }
    }

    async Task OnAppEventMessageReceived(AppEventMessage eventMessage)
    {
        await InvokeAsync(async () =>
        {
            Logger.LogInformation(
                "⚡ App event message: {Type} (Payload: {Value})",
                eventMessage.MessageType, eventMessage.Value);

            if (eventMessage.MessageType is MessageType.LeaveRoom)
            {
                TryLeaveRoom();
                return;
            }

            if (_hubConnection is not null &&
                eventMessage.MessageType is MessageType.CreateOrJoinRoom)
            {
                await SiteJavaScriptModule.CreateOrJoinRoomAsync(
                    eventMessage.Value, eventMessage.TwilioToken!);

                await SiteJavaScriptModule.RequestPictureInPictureAsync(
                    selector: $"""
                    {ElementIds.ParticipantOne} > video
                    """,
                    isPictureInPicture =>
                    {
                        if (isPictureInPicture)
                        {
                            AppState.CameraStatus = CameraStatus.PictureInPicture;
                        }
                        Logger.LogInformation("Entered PiP: {Value}", isPictureInPicture);
                    },
                    onExited: () => AppState.CameraStatus = CameraStatus.InCall);

                await _hubConnection.InvokeAsync(
                    HubEventNames.RoomsUpdated,
                    AppState.ActiveRoomName = eventMessage.Value);
            }

            //if (eventMessage.MessageType is MessageType.SelectCamera &&
            //    (AppState.SelectedCameraId = eventMessage.Value) is { } deviceId)
            //{
            //    if (await SiteJavaScriptModule.StartVideoAsync(deviceId, ElementIds.ParticipantOne))
            //    {
            //        AppState.CameraStatus = CameraStatus.InCall;
            //    }
            //}
        });
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

    Task OnUserConnected(string message) =>
        InvokeAsync(() =>
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

    Task OpenCameraDialog() =>
        ShowDialogAsync<CameraDialog>("Camera Preferences");

    Task OpenRoomDialog() =>
        ShowDialogAsync<RoomDialog>("Available Rooms");

    Task ShowDialogAsync<TDialog>(string title) where TDialog : ComponentBase =>
        Dialog.ShowAsync<TDialog>(title, new DialogParameters
            {
                ["AppEvents"] = _appEvents
            });
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
