using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RestSharp;
using System.Text.RegularExpressions;
using Ionic.Zip;
using Ionic.Crc;

namespace AnnieRecord.riot.model
{
    public partial class Replay
    {
        public static Replay create(Game game, String dir)
        {
            var replay = new Replay(game);
            replay.createFile(dir);

            return replay;
        }

        private IRestResponse findVersion()
        {
            var versionRequest = Riot.Instance.buildSpectateRequest(SPECTATE_METHOD.version, game.id);
            return Riot.Instance.spectateClient.Execute(versionRequest);
        }

        private IRestResponse findMetaData()
        {
            var metaRequest = Riot.Instance.buildSpectateRequest(SPECTATE_METHOD.getGameMetaData, game.id);
            return Riot.Instance.spectateClient.Execute(metaRequest);
        }

        private IRestResponse findChunk(int chunkId)
        {
            var request = Riot.Instance.buildSpectateRequest(SPECTATE_METHOD.getGameDataChunk, game.id);
            request.AddUrlSegment("chunkId", chunkId.ToString());

            return Riot.Instance.spectateClient.Execute(request);
        }

        private IRestResponse findKeyFrae(int keyFrameId)
        {
            var request = Riot.Instance.buildSpectateRequest(SPECTATE_METHOD.getKeyFrame, game.id);
            request.AddUrlSegment("keyFrameId", keyFrameId.ToString());

            return Riot.Instance.spectateClient.Execute(request);
        }
    }
}
