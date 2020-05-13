using Blazing.Twilio.Video.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using Twilio.Jwt.AccessToken;

namespace Blazing.Twilio.Video.Controllers
{
    [
        ApiController,
        Route("api/twilio")
    ]
    public class TwilioController : ControllerBase
    {
        readonly TwilioSettings _twilioSettings;

        public TwilioController(IOptions<TwilioSettings> twilioOptions) =>
            _twilioSettings = twilioOptions?.Value ?? throw new ArgumentException(nameof(twilioOptions));

        [HttpGet("token")]
        public IActionResult GetToken() =>
             new JsonResult(
                 new
                 {
                     token = new Token(
                         _twilioSettings.AccountSid,
                         _twilioSettings.ApiKey,
                         _twilioSettings.ApiSecret,
                         User.Identity.Name ?? Guid.NewGuid().ToString(),
                         grants: new HashSet<IGrant> { new VideoGrant() })
                     .ToJwt()
                 });
    }
}