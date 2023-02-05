// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Blazing.Twilio.Video.Client.Components;
using Blazing.Twilio.Video.Client.Services;

namespace Blazing.Twilio.Video.Client.Shared;

public partial class MainLayout
{
    [Inject] public required AppState AppState { get; set; }
    [Inject] public required ISiteVideoJavaScriptModule JavaScript { get; set; }
    [Inject] public required IDialogService Dialog { get; set; }

    string? SelectedRoom { get; set; }

    AppEventSubject AppEvents { get; set; }

    public MainLayout() =>
        AppEvents = new(OnAppEventMessageReceived);

    async Task OnAppEventMessageReceived(AppEventMessage eventMessage)
    {
        if (eventMessage.MessageType is MessageType.LeftRoom)
        {
            JavaScript.LeaveRoom();
        }

        //eventMessage.MessageType switch
        //{
        //    MessageType.CameraSelected => AppState.SelectedCameraId = eventMessage.Value,
        //    MessageType.LeftRoom =>
        //};

        await Task.CompletedTask;
    }

    void OpenCameraDialog() =>
        Dialog.Show<CameraDialog>("Camera Preferences");

    void OpenRoomDialog() =>
        Dialog.Show<RoomDialog>("Available Rooms");
}
