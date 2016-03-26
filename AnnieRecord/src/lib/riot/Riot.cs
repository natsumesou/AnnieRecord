using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Deserializers;
using AnnieRecord.riot.model;
using System.Net;
using System.Text.RegularExpressions;

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

        private String apiKey;

        internal String spectateBasePath
        {
            get { return "/observer-mode/rest/consumer/"; }
        }

        internal static Riot Instance
        {
            get { return lazy.Value;  }
        }

        private static readonly String RESOURCE_PATTERN = @".*/(.*)/(.*)/(?<id>[0-9]+)/token$";

        private static readonly Lazy<Riot> lazy = new Lazy<Riot>(() => new Riot());

        /// <summary>
        /// Riotクライアントの設定
        /// </summary>
        /// <param name="region">クライアントを操作するRegion</param>
        /// <param name="key">Riot Degveloperポータルで発行したapi key</param>
        public void buildClient(Region region, String key)
        {
            this.region = region;
            this.apiKey = key;
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

        internal static int getResourceIdByPath(String path)
        {
            int id = 0;
            foreach (Match m in Regex.Matches(path, RESOURCE_PATTERN))
            {
                id = Int32.Parse(m.Groups["id"].Value);
            }
            return id;
        }
    }
}
