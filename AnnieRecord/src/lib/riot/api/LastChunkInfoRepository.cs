using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord.riot.model
{
    public partial class LastChunkInfo
    {
        public static LastChunkInfo find(Game game)
        {
            var request = Riot.Instance.buildSpectateRequest(SPECTATE_METHOD.getLastChunkInfo, game.id);
            var response = Riot.Instance.spectateClient.Execute<LastChunkInfo>(request);
            return response.Data;
        }
    }
}
