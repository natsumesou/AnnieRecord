using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace AnnieRecord
{
    class API
    {
        private static readonly Lazy<API> lazy = new Lazy<API>(() => new API());

        public RestClient apiClient {
            get;
            private set;
        }

        public RestClient spectateClient
        {
            get;
            private set;
        }

        public Region region
        {
            get;
            private set;
        }

        private String apiKey
        {
            get { return "9655dc94-8557-43e7-9927-6606c68beb30"; }
        }

        public static API Instance
        {
            get { return lazy.Value;  }
        }

        public void buildClient(Region region)
        {
            this.region = region;
            apiClient = new RestClient(String.Format("https://{0}.api.pvp.net/", this.region.type.ToString()));
            if (region.type == Region.Type.jp)
            {
                spectateClient = new RestClient("http://http://104.160.154.200/");
            }
            else
            {
                spectateClient = new RestClient(String.Format("http://spectator.{0}.lol.riotgames.com/", this.region.type.ToString()));
            }
        }

        public RestRequest buildRequest(String path, Method method = Method.GET)
        {
            var request = new RestRequest(path, method);
            request.AddParameter("api_key", apiKey);
            return request;
        }
    }
}
