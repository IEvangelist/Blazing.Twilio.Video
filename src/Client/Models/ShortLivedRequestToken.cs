// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Client.Models;

/// <summary>
/// An object that represents a short-lived Twilio request token.
/// It expires, by default, in an hour (with a 5-minute buffer), totaling
/// 55 minutes.
/// </summary>
/// <param name="Expiry"></param>
/// <param name="TwilioToken"></param>
public record class ShortLivedRequestToken(
    DateTime Expiry,
    string TwilioToken)
{
    /// <summary>
    /// Gets a value indicating whether or not
    /// the <see cref="TwilioToken"/> is still expired.
    /// </summary>
    [JsonIgnore]
    public bool IsExpired => Expiry > DateTime.Now;
}
