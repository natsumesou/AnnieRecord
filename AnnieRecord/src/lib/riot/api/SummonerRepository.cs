using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace AnnieRecord
{
    public partial class Summoner : BaseModel
    {
        public static Summoner find(String summonerName)
        {
            var request = API.Instance.buildRequest("/api/lol/na/v1.4/summoner/by-name/{summonerName}");
            request.AddUrlSegment("summonerName", summonerName);
            var response = API.Instance.client.Execute<Summoner>(request);
            return response.Data;
        }
    }
}
