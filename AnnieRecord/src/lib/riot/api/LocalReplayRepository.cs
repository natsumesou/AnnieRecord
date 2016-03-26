using Ionic.Crc;
using Ionic.Zip;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnnieRecord.riot.model
{
    public partial class Replay
    {
        private static readonly String PATTERN = @"replay_(?<gameId>[0-9]+)_(?<platformId>.*)\.anr";
        private static readonly String METADATA_KEY = "metadata";

        private FileStream fileStream;
        private ZipOutputStream zipStream;

        private String recordDir;

        private List<String> keys = new List<string>();
        public static Replay find(String dir, String filename)
        {
            String gameId = "";
            String platformId = "";
            foreach (Match m in Regex.Matches(filename, PATTERN))
            {
                gameId = m.Groups["gameId"].Value;
                platformId = m.Groups["platformId"].Value;
            }
            var replay = new Replay(long.Parse(gameId), "", Region.fromPlatformString(platformId));

            using (var zip = ZipFile.Read(dir + "\\" + filename))
            {
                foreach (var e in zip)
                {
                    using (CrcCalculatorStream ccs = e.OpenReader())
                    {
                        using (var ms = new MemoryStream())
                        {
                            ccs.CopyTo(ms);
                            buildReplay(replay, e.FileName, ms.ToArray());
                        }
                    }
                }
            }
            return replay;
        }

        public static bool isExist(Game game, String dir)
        {
            return File.Exists(dir + "\\" + Replay.filenameFromGame(game));
        }

        public void writeChunk(int chunkId)
        {
            write(findChunk(chunkId));
        }

        public void writeKeyFrame(int keyFrameId)
        {
            write(findKeyFrae(keyFrameId));
        }

        public void close()
        {
            zipStream.Close();
            fileStream.Close();
        }

        public bool isWriting()
        {
            try
            {
                var fs = new FileStream(recordDir + "\\" + filename, FileMode.Open);
                fs.Close();
            } catch(IOException)
            {
                return true;
            }
            return false;
        }

        private static void buildReplay(Replay replay, String filename, byte[] bytes)
        {
            if (filename.Contains(METADATA_KEY))
            {
                replay.encryptionKey = Encoding.ASCII.GetString(bytes);
            }
            else if (filename.Contains(SPECTATE_METHOD.version.ToString()))
            {
                replay.version = bytes;
            }
            else if (filename.Contains(SPECTATE_METHOD.getGameMetaData.ToString()))
            {
                replay.metadata = bytes;
            }
            else if (filename.Contains(SPECTATE_METHOD.getGameDataChunk.ToString()))
            {
                replay.chunks.Add(Riot.getResourceIdByPath(filename), bytes);
            }
            else if (filename.Contains(SPECTATE_METHOD.getKeyFrame.ToString()))
            {
                replay.keyFrames.Add(Riot.getResourceIdByPath(filename), bytes);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("what is this?: " + filename);
            }
        }

        private void createFile(String dir)
        {
            recordDir = dir;
            createDirectoryIfNotExist(recordDir);

            fileStream = new FileStream(dir + "\\" + filename, FileMode.Create);
            zipStream = new ZipOutputStream(fileStream);
            zipStream.CompressionLevel = Ionic.Zlib.CompressionLevel.None;

            zipStream.PutNextEntry(METADATA_KEY);
            byte[] buffer = Encoding.ASCII.GetBytes(encryptionKey);
            zipStream.Write(buffer, 0, buffer.Length);

            write(findVersion());
            write(findMetaData());
        }

        private void write(IRestResponse response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return;
            if (keys.Contains(response.toSerializableString()))
                return;
            zipStream.PutNextEntry(response.toSerializableString());
            zipStream.Write(response.RawBytes, 0, response.RawBytes.Length);

            keys.Add(response.toSerializableString());
        }

        private void createDirectoryIfNotExist(String dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}
