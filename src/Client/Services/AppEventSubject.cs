// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Blazing.Twilio.Video.Client.Services;

public sealed class AppEventSubject : IDisposable
{
    readonly Subject<AppEventMessage> _appEventMessageSubject = new();
    readonly IObservable<AppEventMessage> _appEventObservable;
    readonly IDisposable _appEventSubscription;
    readonly Func<AppEventMessage, Task> _observer;

    public AppEventSubject(Func<AppEventMessage, Task> observer)
    {
        _observer = observer;
        _appEventObservable = _appEventMessageSubject.AsObservable();
        _appEventSubscription = _appEventObservable.Subscribe(
            eventMessage => _observer(eventMessage));
    }

    internal void TriggerAppEvent(AppEventMessage eventMessage) =>
        _appEventMessageSubject.OnNext(eventMessage);

    void IDisposable.Dispose() => _appEventSubscription.Dispose();
}

/// <summary>
/// An event message for the app.
/// </summary>
/// <param name="Value">The value of the message.</param>
/// <param name="MessageType">The type of the message.</param>
public readonly record struct AppEventMessage(
    string Value,
    MessageType MessageType);

public enum MessageType
{
    CameraSelected,
    RoomCreatedOrJoined,
    LeftRoom
};