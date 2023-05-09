// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Shared;

/// <summary>
/// The representation of the details of a room.
/// </summary>
/// <param name="Id">The identifier for the room.</param>
/// <param name="Name">The shared name of the room.</param>
/// <param name="ParticipantCount">The number of participants in the room.</param>
/// <param name="MaxParticipants">The maximum number of participants allowed in the room.</param>
public readonly record struct RoomDetails(
    string? Id,
    string? Name,
    int ParticipantCount,
    int MaxParticipants)
{
    /// <summary>
    /// Gets a value indicating whether the room is full.
    /// </summary>
    public bool IsFull => ParticipantCount == MaxParticipants;
}
