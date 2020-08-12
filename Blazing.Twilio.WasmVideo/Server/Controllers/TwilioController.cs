using Blazing.Twilio.WasmVideo.Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Blazing.Twilio.WasmVideo.Server.Controllers
{
    [
        ApiController,
        Route("api/twilio")
    ]
    public class TwilioController : ControllerBase
    {
        [HttpGet("token")]
        public IActionResult GetToken(
            [FromServices] TwilioService twilioService) =>
             new JsonResult(twilioService.GetTwilioJwt(User.Identity.Name));

        [HttpGet("rooms")]
        public async Task<IActionResult> GetRooms(
            [FromServices] TwilioService twilioService) =>
            new JsonResult(await twilioService.GetAllRoomsAsync());
    }
}
