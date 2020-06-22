using Blazing.Twilio.WasmVideo.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blazing.Twilio.WasmVideo.Server.Services
{
    public interface ITwilioService
    {
        TwilioJwt GetTwilioJwt(string? identity);

        ValueTask<IEnumerable<RoomDetails>> GetAllRoomsAsync();
    }
}