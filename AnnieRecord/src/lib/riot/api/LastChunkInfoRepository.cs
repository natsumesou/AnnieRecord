using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord.src.lib.riot.api
{
    public partial class LastChunkInfo : BaseModel
    {
        public static LastChunkInfo find(Game game)
        {
            var request = API.Instance.buildRequest("/observer-mode/rest/consumer/getLastChunkInfo/{platformId}/{gameId}/1/token");
            request.AddUrlSegment("platformId", API.Instance.region.platform.ToString());
            request.AddUrlSegment("gameId", game.id.ToString());
            var response = API.Instance.spectateClient.Execute<LastChunkInfo>(request);
            return response.Data;
        }
    }
}
