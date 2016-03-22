using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord
{
    public partial class Game : BaseModel
    {
        public static Game find(Summoner summoner)
        {
            var request = API.Instance.buildRequest("/observer-mode/rest/consumer/getSpectatorGameInfo/{platformId}/{summonerId}");
            request.AddUrlSegment("summonerId", summoner.id.ToString());
            request.AddUrlSegment("platformId", API.Instance.region.platform.ToString());
            var response = API.Instance.apiClient.Execute<Game>(request);
            return response.Data;
        }
    }
}
