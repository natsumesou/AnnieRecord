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

namespace AnnieRecord
{
    public partial class Replay : BaseModel
    {
        private static readonly String REPLAY_DIR = System.Environment.CurrentDirectory + "\\replays";
        private FileStream fileStream;
        private ZipOutputStream zipStream;

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
            
            using (var zip = ZipFile.Read(REPLAY_DIR + "\\" + filename))
            {
                foreach (var e in zip)
                {
                    using (CrcCalculatorStream ccs = e.OpenReader())
                    {
                        using (var ms = new MemoryStream())
                        {
                            ccs.CopyTo(ms);
                            if (e.FileName.Contains("metadata"))
                            {
                                replay.encryptionKey = Encoding.ASCII.GetString(ms.ToArray());
                            }
                            else if(e.FileName.Contains("version"))
                            {
                                replay.version = ms.ToArray();
                            }
                            else if(e.FileName.Contains("getGameMetaData"))
                            {
                                replay.metadata = ms.ToArray();
                            }
                            else if(e.FileName.Contains("getGameDataChunk"))
                            {
                                replay.chunks.Add(e.FileName, ms.ToArray());
                            }
                            else if (e.FileName.Contains("getKeyFrame"))
                            {
                                replay.keyFrames.Add(e.FileName, ms.ToArray());
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("what is this?: " + e.FileName);
                            }
                        }
                    }
                }
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
            write(response);
        }

        public void writeKeyFrame(int keyFrameId)
        {
            var request = API.Instance.buildRequest("/observer-mode/rest/consumer/getKeyFrame/{platformId}/{gameId}/{keyFrameId}/token");
            request.AddUrlSegment("platformId", API.Instance.region.platform.ToString());
            request.AddUrlSegment("gameId", gameId.ToString());
            request.AddUrlSegment("keyFrameId", keyFrameId.ToString());

            var response = API.Instance.spectateClient.Execute(request);
            write(response);
        }

        private void writeMetaData()
        {
            createDirectoryIfNotExist();
            fileStream = new FileStream(REPLAY_DIR + "\\" + filename, FileMode.Create);
            zipStream = new ZipOutputStream(fileStream);
            zipStream.CompressionLevel = Ionic.Zlib.CompressionLevel.None;
            zipStream.PutNextEntry("metadata");
            byte[] buffer = Encoding.ASCII.GetBytes(encryptionKey);
            zipStream.Write(buffer, 0, buffer.Length);
        }

        private void write(IRestResponse response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return;
            if (keys.Contains(response.toSerializableString()))
                return;
            zipStream.CompressionLevel = Ionic.Zlib.CompressionLevel.None;
            zipStream.PutNextEntry(response.toSerializableString());
            zipStream.Write(response.RawBytes, 0, response.RawBytes.Length);

            keys.Add(response.toSerializableString());
        }

        public void close()
        {
            zipStream.Close();
            fileStream.Close();
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
