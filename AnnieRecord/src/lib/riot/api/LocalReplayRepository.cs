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
        public static readonly String TEMP_FILENAME = "temp.anr";

        private FileStream fileStream;
        private ZipOutputStream zipStream;

        private String recordDir;

        private List<String> keys = new List<string>();

        /// <summary>
        /// リプレイのメタデータのみ格納したインスタンスを返す
        /// </summary>
        /// <param name="dir">リプレイがおかれているディレクトリ</param>
        /// <param name="file">該当のリプレイファイル名</param>
        /// <returns></returns>
        public static Replay find(String dir, String file)
        {
            String gameId = "";
            String platformId = "";
            foreach (System.Text.RegularExpressions.Match m in Regex.Matches(file, PATTERN))
            {
                gameId = m.Groups["gameId"].Value;
                platformId = m.Groups["platformId"].Value;
            }

            Game gameObject;
            using (var sr = new StreamReader(dir + "\\" + file))
            {
                gameObject = Game.fromString(sr.ReadLine());
            }

            var replay = new Replay() { recordDir = dir, game = gameObject };
            return replay;
        }

        /// <summary>
        /// ゲームのリプレイデータをロードする
        /// </summary>
        public void loadPlayData()
        {
            using (var zip = ZipFile.Read(recordDir + "\\" + filename))
            {
                foreach (var e in zip)
                {
                    using (CrcCalculatorStream ccs = e.OpenReader())
                    {
                        using (var ms = new MemoryStream())
                        {
                            ccs.CopyTo(ms);
                            buildReplay(e.FileName, ms.ToArray());
                        }
                    }
                }
            }
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

        public void exportANRFile(Game gameResult)
        {
            close();
            copyByteDataFromTemp(gameResult);
        }

        public bool isWriting()
        {
            try
            {
                var fs = new FileStream(recordDir + "\\" + TEMP_FILENAME, FileMode.Open);
                fs.Close();
            } catch(IOException)
            {
                return true;
            }
            return false;
        }

        public void abort()
        {
            close();
            deleteTempFile();
            File.Delete(recordDir + "\\" + filename);
        }

        private void deleteTempFile()
        {
            File.Delete(recordDir + "\\" + TEMP_FILENAME);
        }

        private void close()
        {
            zipStream.Close();
            fileStream.Close();
        }

        private void copyByteDataFromTemp(Game gameResult)
        {
            var fs = new FileStream(recordDir + "\\" + filename, FileMode.Create);
            var sw = new StreamWriter(fs);
            sw.WriteLine(gameResult.toJsonString());
            sw.Close();
            fs.Close();

            fs = new FileStream(recordDir + "\\" + filename, FileMode.Append);
            var zs = new ZipOutputStream(fs);
            using (var zip = ZipFile.Read(recordDir + "\\" + TEMP_FILENAME))
            {
                foreach (var e in zip)
                {
                    using (CrcCalculatorStream ccs = e.OpenReader())
                    {
                        using (var ms = new MemoryStream())
                        {
                            ccs.CopyTo(ms);
                            var bytes = ms.ToArray();
                            zs.PutNextEntry(e.FileName);
                            zs.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
            }
            zs.Close();
            fs.Close();

            deleteTempFile();
        }

        private void buildReplay(String filename, byte[] bytes)
        {
            if (filename.Contains(SPECTATE_METHOD.version.ToString()))
            {
                this.version = bytes;
            }
            else if (filename.Contains(SPECTATE_METHOD.getGameMetaData.ToString()))
            {
                this.gameMetaData = bytes;
            }
            else if (filename.Contains(SPECTATE_METHOD.getGameDataChunk.ToString()))
            {
                this.chunks.Add(Riot.getResourceIdByPath(filename), bytes);
            }
            else if (filename.Contains(SPECTATE_METHOD.getKeyFrame.ToString()))
            {
                this.keyFrames.Add(Riot.getResourceIdByPath(filename), bytes);
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

            var fs = new FileStream(dir + "\\" + filename, FileMode.Create);
            var sw = new StreamWriter(fs);
            sw.WriteLine(game.toJsonString());
            sw.Close();

            fileStream = new FileStream(dir + "\\" + TEMP_FILENAME, FileMode.Create);
            zipStream = new ZipOutputStream(fileStream);
            zipStream.CompressionLevel = Ionic.Zlib.CompressionLevel.None;

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
