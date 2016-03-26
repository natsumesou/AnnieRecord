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
        public static Replay create(Game game)
        {
            var replay = new Replay(game.id, game.encryptionKey, Riot.Instance.region);
            replay.createFile();

            return replay;
        }

        private IRestResponse findVersion()
        {
            var versionRequest = Riot.Instance.buildSpectateRequest(SPECTATE_METHOD.version, gameId);
            return Riot.Instance.spectateClient.Execute(versionRequest);
        }

        private IRestResponse findMetaData()
        {
            var metaRequest = Riot.Instance.buildSpectateRequest(SPECTATE_METHOD.getGameMetaData, gameId);
            return Riot.Instance.spectateClient.Execute(metaRequest);
        }

        private IRestResponse findChunk(int chunkId)
        {
            var request = Riot.Instance.buildSpectateRequest(SPECTATE_METHOD.getGameDataChunk, gameId);
            request.AddUrlSegment("chunkId", chunkId.ToString());

            return Riot.Instance.spectateClient.Execute(request);
        }

        private IRestResponse findKeyFrae(int keyFrameId)
        {
            var request = Riot.Instance.buildSpectateRequest(SPECTATE_METHOD.getKeyFrame, gameId);
            request.AddUrlSegment("keyFrameId", keyFrameId.ToString());

            return Riot.Instance.spectateClient.Execute(request);
        }
    }
}
