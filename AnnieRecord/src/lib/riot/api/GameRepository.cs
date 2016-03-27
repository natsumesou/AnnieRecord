using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord.riot.model
{
    public partial class Game
    {
        private static readonly String METHOD_VERSION = "v2.2";
        public static Game findCurrent(Summoner summoner)
        {
            var request = Riot.Instance.buildRequest("/observer-mode/rest/consumer/getSpectatorGameInfo/{platformId}/{summonerId}");
            request.AddUrlSegment("summonerId", summoner.id.ToString());
            request.AddUrlSegment("platformId", Riot.Instance.region.platform.ToString());
            var response = Riot.Instance.apiClient.Execute<Game>(request);
            var game = response.Data;
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            foreach(var participant in game.participants)
            {
                if(participant.summonerId == summoner.id)
                {
                    game.player = participant;
                }
            }
            game.platformId = Riot.Instance.region.platform.ToString();
            return game;
        }

        public static Match findMatch(Game game)
        {
            var request = Riot.Instance.buildRequest("/api/lol/{region}/{version}/match/{matchId}");
            request.AddUrlSegment("region", Riot.Instance.region.type.ToString());
            request.AddUrlSegment("version", METHOD_VERSION);
            request.AddUrlSegment("matchId", game.id.ToString());
            var response = Riot.Instance.apiClient.Execute<Match>(request);
            return response.Data;
        }
    }
}
