// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Shared;

/// <summary>
/// A device as returned from the native browser.
/// </summary>
/// <param name="DeviceId">The device identifier.</param>
/// <param name="Label">The label (human readable name) of the device.</param>
public readonly record struct Device(
    string DeviceId,
    string Label);
