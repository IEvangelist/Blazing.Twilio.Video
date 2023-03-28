// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Shared;

/// <summary>
/// The representation of a Twilio JWT.
/// </summary>
/// <param name="Token">The token value.</param>
public readonly record struct TwilioJwt(string? Token);