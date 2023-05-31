// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Blazing.Twilio.Video.Client.Models;

namespace Blazing.Twilio.Video.Client.Shared;

public sealed partial class MainLayout : IDisposable
{
    [Inject] public required NavigationManager NavigationManager { get; set; }
    [Inject] public required HttpClient Http { get; set; }
    [Inject] public required AppState AppState { get; set; }
    [Inject] public required IDialogService Dialog { get; set; }
    [Inject] public required ISnackbar Snackbar { get; set; }
    [Inject] public required ILogger<MainLayout> Logger { get; set; }
    [Inject] public required ILocalStorageService LocalStorage { get; set; }
    [Inject] public required IJSInProcessRuntime JavaScript { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }

    string LeaveRoomQuestion => $"""
        Leave "{AppState.ActiveRoomName ?? "Unknown"}" Room?
        """;

    string ShareRoomQuestion => $"""
        Share "{AppState.ActiveRoomName ?? "Unknown"}" Room?
        """;

    readonly AppEventSubject _appEvents;

    HubConnection? _hubConnection;

    public MainLayout() =>
        _appEvents = new(OnAppEventMessageReceived);

    protected override async Task OnInitializedAsync()
    {
        AppState.StateChanged += OnStateHasChanged;

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
            _ = SiteJavaScriptModule.ExitPictureInPictureAsync(onExited: exited =>
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
                            options.SnackbarVariant = Variant.Filled;
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

    async Task TryShareRoomAsync()
    {
        var roomName = AppState.ActiveRoomName;
        var currentUri = new Uri(Navigation.Uri);
        var uri = new Uri(currentUri, $"/{roomName}");

        try
        {
            // Copy to client browser's clipboard.
            await JavaScript.InvokeVoidAsync
                ("navigator.clipboard.writeText", uri.ToString());

            Snackbar.Add(
                $"""Copied "{uri}" (room URL) to clipboard.""",
                Severity.Success,
                options =>
                {
                    options.CloseAfterNavigation = true;
                    options.SnackbarVariant = Variant.Filled;
                    options.IconSize = Size.Large;
                });
        }
        catch (Exception ex)
        {
            Snackbar.Add(
                $"""Tried copying "{uri}" to clipboard, but failed: {ex}""",
                Severity.Error,
                options =>
                {
                    options.CloseAfterNavigation = true;
                    options.SnackbarVariant = Variant.Filled;
                    options.IconSize = Size.Large;
                });
        }
    }

    void OnStateHasChanged() => StateHasChanged();

    void IDisposable.Dispose() => AppState.StateChanged -= OnStateHasChanged;

    async Task OnPictureInPictureChanged(bool isToggleOn)
    {
        if (isToggleOn)
        {
            await SiteJavaScriptModule.RequestPictureInPictureAsync(
                selector: ElementSelectors.ParticipantOneId.AsVideoSelector(),
                isPictureInPicture =>
                {
                    if (isPictureInPicture)
                    {
                        AppState.CameraStatus = CameraStatus.PictureInPicture;
                    }

                    Logger.LogInformation("Entered PiP: {Value}", isPictureInPicture);
                },
                onExited: () => AppState.CameraStatus = AppState.PreviousCameraStatus);
        }
        else await SiteJavaScriptModule.ExitPictureInPictureAsync(onExited: exited =>
        {
            if (exited && AppState is { CameraStatus: CameraStatus.PictureInPicture })
            {
                AppState.CameraStatus = AppState.PreviousCameraStatus;
            }
            else
            {
                Logger.LogWarning("😶 Unable to exit picture-in-picture mode.");
            }
        });
    }

    async Task OnAppEventMessageReceived(AppEventMessage eventMessage)
    {
        await InvokeAsync(async () =>
        {
            Logger.LogInformation(
                "⚡ App event message: {Type} (Payload: {Value})",
                eventMessage.MessageType, eventMessage.Value);

            if (eventMessage is { MessageType: MessageType.LeaveRoom })
            {
                TryLeaveRoom();
                return;
            }

            if (_hubConnection is not null &&
                eventMessage is { MessageType: MessageType.CreateOrJoinRoom } &&
                eventMessage.TwilioToken is { } requestToken &&
                    await SiteJavaScriptModule.CreateOrJoinRoomAsync(
                        eventMessage.Value, requestToken))
            {
                TrySetShortLivedRequestToken(requestToken);

                await SiteJavaScriptModule.RequestPictureInPictureAsync(
                    selector: ElementSelectors.ParticipantOneId.AsVideoSelector(),
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

            if (eventMessage is { MessageType: MessageType.SelectCamera } &&
                AppState.SelectedCameraId is { } deviceId)
            {
                if (AppState.CameraStatus switch
                {
                    CameraStatus.PictureInPicture or
                    CameraStatus.InCall or
                    CameraStatus.Previewing => true,
                    _ => false
                })
                {
                    return;
                }

                var selector = AppState is { CameraStatus: CameraStatus.RequestingPreview }
                    ? ElementSelectors.CameraPreviewId
                    : ElementSelectors.ParticipantOneId;

                if (await SiteJavaScriptModule.StartVideoAsync(deviceId, selector))
                {
                    AppState.CameraStatus = CameraStatus.InCall;
                }
            }
        });
    }

    void TrySetShortLivedRequestToken(string requestToken)
    {
        if (LocalStorage.GetItem<ShortLivedRequestToken>(
            StorageKeys.ShortLivedRequestToken) is { IsExpired: true })
        {
            LocalStorage.SetItem(
                StorageKeys.ShortLivedRequestToken,
                new ShortLivedRequestToken(
                    Expiry: DateTime.Now.AddMinutes(55), TwilioToken: requestToken));
        }
    }

    Task OnRoomAdded(string roomName) =>
        InvokeAsync(async () =>
        {
            AppState.Rooms = await Http.GetFromJsonAsync<HashSet<RoomDetails>>("api/twilio/rooms")
                ?? new();

            Logger.LogInformation(LogMessageTemplates.OnRoomAdded, roomName);

            Snackbar.Add(
                LogMessageTemplates.OnRoomAdded.Replace(
                    "{RoomName}", roomName),
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
            Logger.LogInformation(LogMessageTemplates.OnUserConnected, message);

            Snackbar.Add(
                LogMessageTemplates.OnUserConnected.Replace(
                    "{Message}", message),
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
    internal const string OnRoomAdded = "{RoomName} was just created.";

    /// <summary>
    /// If not passed as the <c>message</c> argument of
    /// the <see cref="ILogger{TCategoryName}.LogInformation"/> method,
    /// call <c>LogMessageTemplates.OnUserConnected.Replace("{Message}", message);</c>
    /// </summary>
    internal const string OnUserConnected = "{Message}";
}
