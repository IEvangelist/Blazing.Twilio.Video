// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Server.Extensions;

internal static class WebApplicationExtensions
{
    /// <summary>
    /// Maps the following APIs on the <c>"api/twilio"</c> route:
    /// <list type="bullet">
    /// <item>
    /// HTTP GET: <c>"api/twilio/token"</c> - returns a <see cref="TwilioJwt"/> to use for future API calls.
    /// </item>
    /// <item>
    /// HTTP GET: <c>"api/twilio/rooms"</c> - returns a collection of <see cref="RoomDetails"/> (only in-progress rooms).
    /// </item>
    /// </list>
    /// </summary>
    internal static WebApplication MapTwilioApi(this WebApplication app)
    {
        var twilioApi = app.MapGroup("api/twilio");

        twilioApi.MapGet(
            "token",
            static (TwilioService twilioService, ClaimsPrincipal user) =>
                twilioService.GetTwilioJwt(user?.Identity?.Name));

        twilioApi.MapGet(
            "rooms",
            static (TwilioService twilioService) => twilioService.GetAllRoomsAsync());

        return app;
    }
}
