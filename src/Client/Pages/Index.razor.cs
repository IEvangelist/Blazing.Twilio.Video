// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Pages;

public sealed partial class Index : IDisposable
{
    [Inject]
    public required ISiteVideoJavaScriptModule JavaScript { get; set; }

    [Inject]
    public required AppState AppState { get; set; }

    [CascadingParameter]
    public required AppEventSubject AppEvents { get; set; }

    protected override async Task OnInitializedAsync()
    {
        AppState.StateChanged += StateHasChanged;
        await JavaScript.InitializeModuleAsync();

        if (AppState.SelectedCameraId is { } deviceId)
        {
            JavaScript.StartVideo(deviceId, ElementIds.ParticipantOne);
        }
    }

    void IDisposable.Dispose() => AppState.StateChanged -= StateHasChanged;
}