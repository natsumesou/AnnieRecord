using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace AnnieRecord.riot.model
{
    public partial class Summoner
    {
        public static Summoner find(String summonerName)
        {
            var request = Riot.Instance.buildRequest("/api/lol/{region}/v1.4/summoner/by-name/{summonerName}");
            request.AddUrlSegment("summonerName", summonerName);
            request.AddUrlSegment("region", Riot.Instance.region.type.ToString());
            request.RootElement = summonerName.ToLower().Replace(" ", String.Empty);
            var response = Riot.Instance.apiClient.Execute<Summoner>(request);
            return response.Data;
        }
    }
}
