using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord.riot.model
{
    public partial class Game
    {
        public static Game findCurrent(Summoner summoner)
        {
            var request = Riot.Instance.buildRequest("/observer-mode/rest/consumer/getSpectatorGameInfo/{platformId}/{summonerId}");
            request.AddUrlSegment("summonerId", summoner.id.ToString());
            request.AddUrlSegment("platformId", Riot.Instance.region.platform.ToString());
            var response = Riot.Instance.apiClient.Execute<Game>(request);
            return response.Data;
        }
    }
}
