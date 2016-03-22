using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AnnieRecord
{
    public partial class Replay : BaseModel
    {
        private static readonly String REPLAY_DIR = System.Environment.CurrentDirectory + "\replays";
        public static Replay find(Game game)
        {
            var metaRequest = API.Instance.buildRequest("/observer-mode/rest/consumer/getGameMetaData/{platformId}/{gameId}/1/token");
            metaRequest.AddUrlSegment("platformId", API.Instance.region.platform.ToString());
            metaRequest.AddUrlSegment("gameId", game.id.ToString());
            var metaResponse = API.Instance.spectateClient.Execute(metaRequest);

            var versionRequest = API.Instance.buildRequest("/observer-mode/rest/consumer/version");
            var versionResponse = API.Instance.spectateClient.Execute(versionRequest);

            var replay = new Replay(game.id, versionResponse.Content, game.encryptionKey, metaResponse.Content, game.gameStartTime);
            return replay;
        }

        public static Replay find(String fileName)
        {
            using (var sr = new StreamReader(REPLAY_DIR + "\\" + fileName))
            {
                XmlSerializer xs = new XmlSerializer(typeof(Replay));
                return (Replay)xs.Deserialize(sr);
            }
        }

        public void findChunk(int chunkId)
        {
            var request = API.Instance.buildRequest("/observer-mode/rest/consumer/getGameDataChunk/{platformId}/{gameId}/{chunkId}/token");
            request.AddUrlSegment("platformId", API.Instance.region.platform.ToString());
            request.AddUrlSegment("gameId", gameId.ToString());
            request.AddUrlSegment("chunkId", chunkId.ToString());

            var response = API.Instance.spectateClient.Execute(request);
            chunks.Add(chunkId, response.Content);
        }

        public void findKeyFrame(int keyFrameId)
        {
            var request = API.Instance.buildRequest("/observer-mode/rest/consumer/getKeyFrame/{platformId}/{gameId}/{keyFrameId}/token");
            request.AddUrlSegment("platformId", API.Instance.region.platform.ToString());
            request.AddUrlSegment("gameId", gameId.ToString());
            request.AddUrlSegment("keyFrameId", keyFrameId.ToString());

            var response = API.Instance.spectateClient.Execute(request);
            keyFrames.Add(keyFrameId, response.Content);
        }

        public void save()
        {
            createDirectoryIfNotExist();
            XmlSerializer xs = new XmlSerializer(typeof(Replay));
            TextWriter tw = new StreamWriter(REPLAY_DIR + "\\" + fileName());
            xs.Serialize(tw, this);
        }

        public void createDirectoryIfNotExist()
        {
            if(!Directory.Exists(REPLAY_DIR))
            {
                Directory.CreateDirectory(REPLAY_DIR);
            }
        }
    }
}
