// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Server.Services;

internal sealed class TwilioService
{
    readonly TwilioSettings _twilioSettings;

    public TwilioService(MicrosoftOptions.IOptions<TwilioSettings> twilioOptions)
    {
        _twilioSettings =
            twilioOptions?.Value ??
            throw new ArgumentNullException(nameof(twilioOptions));

        TwilioClient.Init(
            _twilioSettings.ApiKey ?? "no-key",
            _twilioSettings.ApiSecret ?? "no-secret");
    }

    public TwilioJwt GetTwilioJwt(string? identity)
    {
        var token = new Token(
            _twilioSettings.AccountSid,
            _twilioSettings.ApiKey,
            _twilioSettings.ApiSecret,
            identity ?? GetName(),
            grants: new HashSet<IGrant> { new VideoGrant() });

        return new(Token: token.ToJwt());
    }

    public async ValueTask<IEnumerable<RoomDetails>> GetAllRoomsAsync()
    {
        var rooms = await RoomResource.ReadAsync(new ReadRoomOptions
        {
            Status = RoomResource.RoomStatusEnum.Completed
        });
        var tasks = rooms.Select(
            room => GetRoomDetailsAsync(
                room,
                ParticipantResource.ReadAsync(
                    room.Sid,
                    ParticipantStatus.Connected)));

        return await Task.WhenAll(tasks);

        static async Task<RoomDetails> GetRoomDetailsAsync(
            RoomResource room,
            Task<ResourceSet<ParticipantResource>> participantTask)
        {
            var participants = await participantTask;
            return new RoomDetails(
                room.Sid,
                room.UniqueName,
                room.MaxParticipants ?? 0,
                participants.Count());
        }
    }

    #region Borrowed from https://github.com/twilio/video-quickstart-js/blob/1.x/server/randomname.js

    static readonly string[] s_adjectives =
    {
        "Abrasive", "Brash", "Callous", "Daft", "Eccentric", "Feisty", "Golden",
        "Holy", "Ignominious", "Luscious", "Mushy", "Nasty",
        "OldSchool", "Pompous", "Quiet", "Rowdy", "Sneaky", "Tawdry",
        "Unique", "Vivacious", "Wicked", "Xenophobic", "Yawning", "Zesty"
    };

    static readonly string[] s_firstNames =
    {
        "Anna", "Bobby", "Cameron", "Danny", "Emmett", "Frida", "Gracie", "Hannah",
        "Isaac", "Jenova", "Kendra", "Lando", "Mufasa", "Nate", "Owen", "Penny",
        "Quincy", "Roddy", "Samantha", "Tammy", "Ulysses", "Victoria", "Wendy",
        "Xander", "Yolanda", "Zelda"
    };

    static readonly string[] s_lastNames =
    {
        "Anchorage", "Berlin", "Cucamonga", "Davenport", "Essex", "Fresno",
        "Gunsight", "Hanover", "Indianapolis", "Jamestown", "Kane", "Liberty",
        "Minneapolis", "Nevis", "Oakland", "Portland", "Quantico", "Raleigh",
        "SaintPaul", "Tulsa", "Utica", "Vail", "Warsaw", "XiaoJin", "Yale",
        "Zimmerman"
    };

    string GetName() =>
        $"{s_adjectives.RandomElement()} {s_firstNames.RandomElement()} {s_lastNames.RandomElement()}";

    #endregion
}

file static class StringArrayExtensions
{
    internal static string RandomElement(this IReadOnlyList<string> array) =>
        array[Random.Shared.Next(array.Count)];
}