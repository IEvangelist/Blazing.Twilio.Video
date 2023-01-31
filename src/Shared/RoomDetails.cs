// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Shared;

/// <summary>
/// 
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="ParticipantCount"></param>
/// <param name="MaxParticipants"></param>
public readonly record struct RoomDetails(
    string? Id,
    string? Name,
    int ParticipantCount,
    int MaxParticipants);
