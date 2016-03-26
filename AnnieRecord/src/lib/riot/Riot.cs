using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Deserializers;
using AnnieRecord.riot.model;

namespace AnnieRecord.riot
{
    internal enum SPECTATE_METHOD
    {
        version,
        getGameMetaData,
        getLastChunkInfo,
        getGameDataChunk,
        getKeyFrame
    }

    public class Riot
    {
        public Region region
        {
            get;
            private set;
        }

        internal RestClient apiClient {
            get;
            private set;
        }

        internal RestClient spectateClient
        {
            get;
            private set;
        }

        private String apiKey
        {
            get { return "9655dc94-8557-43e7-9927-6606c68beb30"; }
        }

        internal String spectateBasePath
        {
            get { return "/observer-mode/rest/consumer/"; }
        }

        internal static Riot Instance
        {
            get { return lazy.Value;  }
        }

        private static readonly Lazy<Riot> lazy = new Lazy<Riot>(() => new Riot());

        public void buildClient(Region region)
        {
            this.region = region;
            apiClient = new RestClient(String.Format("https://{0}.api.pvp.net/", this.region.type.ToString()));

            if (region.type == Region.Type.jp)
            {
                spectateClient = new RestClient("http://104.160.154.200/");
            }
            else
            {
                spectateClient = new RestClient(String.Format("http://spectator.{0}.lol.riotgames.com/", this.region.type.ToString()));
            }
        }

        internal RestRequest buildRequest(String path, Method method = Method.GET)
        {
            var request = new RestRequest(path, method);
            request.AddParameter("api_key", apiKey);
            request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            return request;
        }

        internal RestRequest buildSpectateRequest(SPECTATE_METHOD method, long gameId)
        {
            RestRequest request;
            String path = spectateBasePath + method;

            switch (method) {
                case SPECTATE_METHOD.version:
                    break;
                case SPECTATE_METHOD.getGameMetaData:
                case SPECTATE_METHOD.getLastChunkInfo:
                    path += "/{platformId}/{gameId}/1/token";
                    break;
                case SPECTATE_METHOD.getGameDataChunk:
                    path += "/{platformId}/{gameId}/{chunkId}/token";
                    break;
                case SPECTATE_METHOD.getKeyFrame:
                    path += "/{platformId}/{gameId}/{keyFrameId}/token";
                    break;
                default:
                    throw new Exception(method + " method not supported");
            }
            request = Riot.Instance.buildRequest(path);
            request.AddUrlSegment("platformId", region.platform.ToString());
            request.AddUrlSegment("gameId", gameId.ToString());

            return request;
        }
    }
}
