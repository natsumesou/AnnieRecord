﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord
{
    public class Client
    {
        public static readonly String CLIENT_NAME = "League of Legends real.exe";
        private static readonly String TEMP_FILE_NAME = "AnnieRecord_currentGameInfo.txt";
        private static readonly String defaultDir = @"C:\Riot Games\League of Legends\RADS\solutions\lol_game_client_sln\releases\";
        private static String _clientDir;
        public static String clientDir
        {
            get {
                if (_clientDir == null)
                {
                    var directories = Directory.GetDirectories(defaultDir);
                    var versions = new List<Version>();
                    foreach(var dir in directories)
                    {
                        versions.Add(new Version(dir.Remove(0, defaultDir.Length)));
                    }
                    Version latestVersion = new Version("0.0.0.001");
                    foreach(var ver in versions)
                    {
                        if (ver.CompareTo(latestVersion) > 0)
                            latestVersion = ver;
                    }
                    _clientDir = defaultDir + latestVersion.ToString() + @"\deploy";
                }
                return _clientDir;
            }
        }

        public static Game createGameFromClientInfo()
        {
            string gameId = "";
            string encryptionKey = "";
            string platformId = "";
            using (var sr = new StreamReader(clientDir + "\\" + TEMP_FILE_NAME))
            {
                string line;
                while ((line = sr.ReadLine()) != null) {
                    if(sr.EndOfStream)
                    {
                        var split = line.Split(null);
                        gameId = split[split.Length - 2];
                        encryptionKey = split[split.Length - 3];
                        platformId = split[split.Length - 1];
                    }
                }
            }
            return Game.fromLocalClient(long.Parse(gameId), encryptionKey, platformId);
        }

        public static void LaunchReplay(Replay replay)
        {
            var arguments = "";
            var spectArgs = String.Format("spectator {0}:{1} {2} {3} {4}", Server.HOST, Server.PORT, replay.encryptionKey, replay.gameId, replay.region.platform);
            String[] args = new string[] { "8394", "LoLPatcher.exe", "", spectArgs };
            foreach (string arg in args)
                arguments += "\"" + arg + "\" ";

            var processInfo = new ProcessStartInfo();
            processInfo.WorkingDirectory = clientDir;
            processInfo.FileName = CLIENT_NAME;
            processInfo.Arguments = arguments;
            Process p = Process.Start(processInfo);
        }
    }
}
