// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Pages;

public sealed partial class Index
{
    [Inject] public required AppState AppState { get; set; }
    [Inject] public required ILogger<Index> Logger { get; set; }
    [Inject] public required ILocalStorageService LocalStorage { get; set; }
    [Inject] public required HttpClient Http { get; set; }

    [CascadingParameter] public required AppEventSubject AppEvents { get; set; }

    [Parameter] public required string? RoomName { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender is false && AppState.SelectedCameraId is { } deviceId)
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

            if (RoomName is not null &&
                AppState.ActiveRoomName != RoomName &&
                AppState.ShortLivedRequestToken is null or { IsExpired: true } &&
                    await Http.GetFromJsonAsync<TwilioJwt>("api/twilio/token") is { Token.Length: > 0 } jwt)
            {
                AppEvents.TriggerAppEvent(new AppEventMessage(
                    Value: RoomName,
                    TwilioToken: jwt.Token,
                    MessageType: MessageType.CreateOrJoinRoom));
            }
        }
    }
}