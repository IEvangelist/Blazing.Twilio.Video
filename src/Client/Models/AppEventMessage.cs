// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Models;

/// <summary>
/// An event message for the app.
/// </summary>
/// <param name="Value">The value of the message.</param>
/// <param name="MessageType">The type of the message.</param>
/// <param name="TwilioToken">The Twilio Token (JWT).</param>
public readonly record struct AppEventMessage(
    string Value,
    MessageType MessageType,
    string? TwilioToken = default);
