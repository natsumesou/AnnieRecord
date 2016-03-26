using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord.riot.model
{
    public class GameClient
    {
        public static readonly String MOCK_CLIENT_NAME = "League of Legends.exe";
        public static readonly String CLIENT_NAME = "League of Legends.exe";
        private static readonly String TEMP_FILE_NAME = "AnnieRecord_currentGameInfo.txt";
        private static readonly String BASE_PATH = @"\League of Legends\RADS\solutions\lol_game_client_sln\releases\";

        private static String _clientDir;
        private static String clientDir(String clientBaseDir) {
            if (_clientDir == null)
            {
                var directories = Directory.GetDirectories(clientBaseDir + BASE_PATH);
                var versions = new List<Version>();
                foreach(var dir in directories)
                {
                    versions.Add(new Version(dir.Remove(0, (clientBaseDir + BASE_PATH).Length)));
                }
                Version latestVersion = new Version("0.0.0.001");
                foreach(var ver in versions)
                {
                    if (ver.CompareTo(latestVersion) > 0)
                        latestVersion = ver;
                }
                _clientDir = clientBaseDir + BASE_PATH + latestVersion.ToString() + @"\deploy";
            }
            return _clientDir;
        }

        [Obsolete]
        public static Game createGameFromClientInfo(Region region, String clientBaseDir)
        {
            string gameId = "";
            string encryptionKey = "";
            string platformId = "";
            using (var sr = new StreamReader(clientDir(clientBaseDir) + "\\" + TEMP_FILE_NAME))
            {
                string line;
                while ((line = sr.ReadLine()) != null) {
                    if(sr.EndOfStream)
                    {
                        var split = line.Split(null);
                        if (split[0].Equals("spectator"))
                        {
                            gameId = split[split.Length - 2];
                            encryptionKey = split[split.Length - 3];
                            platformId = split[split.Length - 1];
                        } else
                        {
                            gameId = split[split.Length - 1];
                            encryptionKey = split[split.Length - 2];
                            platformId = region.platform.ToString();
                        }
                    }
                }
            }
            return Game.fromLocalClient(long.Parse(gameId), encryptionKey, platformId);
        }

        /// <summary>
        /// リプレイの起動
        /// </summary>
        /// <param name="replay">リプレイデータが入ったModel</param>
        /// <param name="clientBaseDir">LoLのクライアントディレクトリ</param>
        public static void LaunchReplay(Replay replay, String clientBaseDir)
        {
            var arguments = "";
            var spectArgs = String.Format("spectator {0}:{1} {2} {3} {4}", Server.HOST, Server.localPort, replay.encryptionKey, replay.gameId, replay.region.platform);
            String[] args = new string[] { "8394", "LoLPatcher.exe", "", spectArgs };
            foreach (string arg in args)
                arguments += "\"" + arg + "\" ";

            var processInfo = new ProcessStartInfo();
            processInfo.WorkingDirectory = clientDir(clientBaseDir);
            processInfo.FileName = MOCK_CLIENT_NAME;
            processInfo.Arguments = arguments;
            Process p = Process.Start(processInfo);
        }
    }
}
