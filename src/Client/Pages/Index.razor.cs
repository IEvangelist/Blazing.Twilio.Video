// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Pages;

public sealed partial class Index
{
    [Inject] public required AppState AppState { get; set; }

    [Inject] public required ILogger<Index> Logger { get; set; }

    [CascadingParameter] public required AppEventSubject AppEvents { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender is false && AppState.SelectedCameraId is { } deviceId)
        {
            if (AppState is
                {
                    CameraStatus:
                        CameraStatus.PictureInPicture or
                        CameraStatus.InCall or
                        CameraStatus.Previewing
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
    }
}