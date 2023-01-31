// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Blazing.Twilio.Video.Server;

internal static class RouteGroupBuilderExtensions
{
    internal static RouteGroupBuilder MapTwilioApi(this RouteGroupBuilder group)
    {
        group.MapGet(
            "token",
            static (TwilioService twilioService, ClaimsPrincipal user) =>
                twilioService.GetTwilioJwt(user?.Identity?.Name));
        
        group.MapGet(
            "rooms",
            static (TwilioService twilioService) => twilioService.GetAllRoomsAsync());

        return group;
    }
}
