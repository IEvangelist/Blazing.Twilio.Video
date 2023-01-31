// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Shared;

/// <summary>
/// 
/// </summary>
/// <param name="DeviceId"></param>
/// <param name="Label"></param>
public readonly record struct Device(
    string DeviceId,
    string Label);
