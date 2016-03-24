using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RestSharp;
using System.Text.RegularExpressions;

namespace AnnieRecord
{
    public partial class Replay : BaseModel
    {
        private static readonly String REPLAY_DIR = System.Environment.CurrentDirectory + "\\replays";
        private List<String> keys = new List<string>();

        public static Replay find(Game game)
        {
            var metaRequest = API.Instance.buildRequest("/observer-mode/rest/consumer/getGameMetaData/{platformId}/{gameId}/1/token");
            metaRequest.AddUrlSegment("platformId", API.Instance.region.platform.ToString());
            metaRequest.AddUrlSegment("gameId", game.id.ToString());
            var metaResponse = API.Instance.spectateClient.Execute(metaRequest);

            var versionRequest = API.Instance.buildRequest("/observer-mode/rest/consumer/version");
            var versionResponse = API.Instance.spectateClient.Execute(versionRequest);

            var replay = new Replay(game.id, game.encryptionKey, API.Instance.region);
            replay.writeMetaData();
            replay.write(metaResponse);
            replay.write(versionResponse);

            return replay;
        }

        public static bool isExist(Game game)
        {
            return File.Exists(REPLAY_DIR + "\\" + Replay.filenameFromGame(game));
        }

        public static Replay find(String filename)
        {
            var pattern = @"replay_(?<gameId>[0-9]+)_(?<platformId>.*)\.anr";
            String gameId = "";
            String platformId = "";
            foreach (Match m in Regex.Matches(filename, pattern))
            {
                gameId = m.Groups["gameId"].Value;
                platformId = m.Groups["platformId"].Value;
            }
            var replay = new Replay(long.Parse(gameId), "", Region.fromPlatformString(platformId));

            List<byte[]> lineBytes = new List<byte[]>();
            var bytes = File.ReadAllBytes(REPLAY_DIR + "\\" + filename);

            int binaryIndex = 0;
            for (var i = 0; i < bytes.Length; i++)
            {
                var b = bytes[i];
                if (bytes[i] == (byte)'\n')
                {
                    lineBytes.Add(bytes.Skip(binaryIndex).Take(i - binaryIndex).ToArray());
                    binaryIndex = i + 1;
                }
            }

            var key = "";
            byte[] body = new byte[0];
            for (var i = 0; i < lineBytes.Count; i++)
            {
                var lineStr = Encoding.ASCII.GetString(lineBytes[i]);
                if (i == 0)
                {
                    replay.buildEncryptionKey(lineStr);
                }
                if (lineStr.IndexOf("GET ") >= 0)
                {
                    if (key.Length > 0 && body.Length > 0)
                    {
                        replay.data.Add(key, body);
                        if(key.IndexOf("getGameDataChunk") >= 0)
                        {
                            replay.lastChunkPath = key;
                        }
                        if (key.IndexOf("getKeyFrame") >= 0)
                        {
                            replay.lastkeyFramePath = key;
                        }
                    }
                    key = lineStr.Replace("\r", String.Empty);
                    body = new byte[0];
                } else
                {
                    body = merge(body, lineBytes[i]);
                    body = merge(body, new byte[] { (byte)'\n' });
                }
            }
            replay.data.Add(key, body);
            if (key.IndexOf("getGameDataChunk") >= 0)
            {
                replay.lastChunkPath = key;
            }
            if (key.IndexOf("getKeyFrame") >= 0)
            {
                replay.lastkeyFramePath = key;
            }

            return replay;
        }

        private static byte[] merge(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }

        public void writeLastChunkInfo()
        {
            write(LastChunkInfo.lastResponse);
        }

        public void writeChunk(int chunkId)
        {
            var request = API.Instance.buildRequest("/observer-mode/rest/consumer/getGameDataChunk/{platformId}/{gameId}/{chunkId}/token");
            request.AddUrlSegment("platformId", API.Instance.region.platform.ToString());
            request.AddUrlSegment("gameId", gameId.ToString());
            request.AddUrlSegment("chunkId", chunkId.ToString());

            var response = API.Instance.spectateClient.Execute(request);
            write(response, true);
        }

        public void writeKeyFrame(int keyFrameId)
        {
            var request = API.Instance.buildRequest("/observer-mode/rest/consumer/getKeyFrame/{platformId}/{gameId}/{keyFrameId}/token");
            request.AddUrlSegment("platformId", API.Instance.region.platform.ToString());
            request.AddUrlSegment("gameId", gameId.ToString());
            request.AddUrlSegment("keyFrameId", keyFrameId.ToString());

            var response = API.Instance.spectateClient.Execute(request);
            write(response, true);
        }

        private void writeMetaData()
        {
            createDirectoryIfNotExist();
            using (StreamWriter outputFile = new StreamWriter(REPLAY_DIR + "\\" + filename))
            {
                outputFile.WriteLine(encryptionKey);
            }
        }

        private void write(IRestResponse response, bool isBinary = false)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return;
            if (keys.Contains(response.toSerializableString()))
                return;
            using (StreamWriter outputFile = new StreamWriter(REPLAY_DIR + "\\" + filename, true))
            {
                outputFile.WriteLine(response.toSerializableString());
            }
            using (StreamWriter outputFile = new StreamWriter(REPLAY_DIR + "\\" + filename, true))
            {
                if (isBinary)
                {
                    outputFile.BaseStream.Flush();
                    outputFile.BaseStream.Write(response.RawBytes, 0, response.RawBytes.Length);
                    byte[] newline = new byte[] { (byte)'\n' };
                    outputFile.BaseStream.Write(newline, 0, newline.Length);
                }
                else
                {
                    outputFile.WriteLine(response.Content);
                }
            }
            keys.Add(response.toSerializableString());

        }

        private void createDirectoryIfNotExist()
        {
            if (!Directory.Exists(REPLAY_DIR))
            {
                Directory.CreateDirectory(REPLAY_DIR);
            }
        }
    }
}
