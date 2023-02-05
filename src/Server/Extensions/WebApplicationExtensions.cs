// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Server.Extensions;

internal static class WebApplicationExtensions
{
    internal static WebApplication MapTwilioApi(this WebApplication app)
    {
        var group = app.MapGroup("api/twilio");
        group.MapGet(
            "token",
            static (TwilioService twilioService, ClaimsPrincipal user) =>
                twilioService.GetTwilioJwt(user?.Identity?.Name));

        group.MapGet(
            "rooms",
            static (TwilioService twilioService) => twilioService.GetAllRoomsAsync());

        return app;
    }
}
