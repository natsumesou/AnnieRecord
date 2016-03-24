using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord
{
    public partial class LastChunkInfo : BaseModel
    {
        public static LastChunkInfo find(Game game)
        {
            var request = API.Instance.buildRequest("/observer-mode/rest/consumer/getLastChunkInfo/{platformId}/{gameId}/30000/token");
            request.AddUrlSegment("platformId", API.Instance.region.platform.ToString());
            request.AddUrlSegment("gameId", game.id.ToString());
            var response = API.Instance.spectateClient.Execute<LastChunkInfo>(request);
            lastResponse = response;
            return response.Data;
        }
    }
}
