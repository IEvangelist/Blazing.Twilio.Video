// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Services;

public sealed class AppEventSubject : IDisposable
{
    readonly Subject<AppEventMessage> s_appEventMessageSubject = new();
    readonly IObservable<AppEventMessage> _appEventObservable;
    readonly IDisposable _appEventSubscription;
    readonly Func<AppEventMessage, Task> _observer;

    public AppEventSubject(Func<AppEventMessage, Task> observer)
    {
        _observer = observer;
        _appEventObservable = s_appEventMessageSubject.AsObservable();
        _appEventSubscription = _appEventObservable.Subscribe(
            eventMessage => _observer(eventMessage));
    }

    internal void TriggerAppEvent(AppEventMessage eventMessage) =>
        s_appEventMessageSubject.OnNext(eventMessage);

    void IDisposable.Dispose() => _appEventSubscription.Dispose();
}
