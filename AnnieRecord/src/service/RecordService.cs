using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Threading;
using AnnieRecord.riot;
using AnnieRecord.riot.model;

namespace AnnieRecord
{
    public class RecordService
    {
        private Summoner summoner;
        private Game game;
        private Thread thread;
        private String recordDir;
        private String clientDir;
        private Replay replay;

        ///
        /// <summary>
        /// リプレイファイルを作成する
        /// </summary>
        /// <param name="s">録画対象のサモナーモデル</param>
        /// <param name="clientDirectory">LoLのクライアントがインストールされているディレクトリ(デフォルトはC:\Riot Games)</param>
        /// <param name="recordDirectory">録画ファイルを保存するディレクトリ</param>
        public RecordService(Summoner s, String clientDirectory, String recordDirectory)
        {
            this.summoner = s;
            this.recordDir = recordDirectory;
            this.clientDir = clientDirectory;
        }

        /// <summary>
        /// LoLのクライアントが起動したら自動で録画処理を行う
        /// </summary>
        public void watch()
        {
            ManagementEventWatcher startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            startWatch.EventArrived += new EventArrivedEventHandler(startWatch_EventArrived);
            startWatch.Start();

            ManagementEventWatcher stopWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
            stopWatch.EventArrived += new EventArrivedEventHandler(stopWatch_EventArrived);
            stopWatch.Start();
        }

        /// <summary>
        /// LoLクライアントの起動を感知せず手動で録画開始する
        /// </summary>
        [Obsolete]
        public void startRecord()
        {
            findAndPrepareGameInfo();
        }

        /// <summary>
        /// リプレイファイルが書き込み中かどうか
        /// </summary>
        /// <returns></returns>
        public bool isWriting()
        {
            if (replay == null)
                return false;
            return replay.isWriting();
        }

        private void startWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (!e.NewEvent.Properties["ProcessName"].Value.Equals(GameClient.CLIENT_NAME))
                return;
            findAndPrepareGameInfoFromLocal();
        }

        private void stopWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (!e.NewEvent.Properties["ProcessName"].Value.Equals(GameClient.CLIENT_NAME))
                return;
        }

        private void findAndPrepareGameInfoFromLocal()
        {
            game = GameClient.createGameFromClientInfo(Riot.Instance.region, clientDir);
            if (Replay.isExist(game, recordDir))
                return;
            replay = Replay.create(game, recordDir);

            thread = new Thread(() => startRecord(replay));
            thread.IsBackground = true;
            thread.Start();
        }

        private void findAndPrepareGameInfo()
        {
            game = Game.findCurrent(summoner);
            if(game.id == 0)
            {
                System.Diagnostics.Debug.WriteLine("Currently not playing game");
                return;
            }
            var replay = Replay.create(game, recordDir);

            thread = new Thread(() => startRecord(replay));
            thread.IsBackground = true;
            thread.Start();
        }

        private void startRecord(Replay r)
        {
            System.Diagnostics.Debug.WriteLine("start recording");
            replay = r;
            int chunkId = 1;
            int keyFrameId = 1;
            while(true)
            {
                replay.writeChunk(chunkId++);
                replay.writeKeyFrame(keyFrameId++);

                var lastChunkInfo = LastChunkInfo.find(game);

                if (lastChunkInfo.isLastChunk())
                {
                    replay.writeChunk(lastChunkInfo.chunkId);
                    replay.close();
                    System.Diagnostics.Debug.WriteLine("end recording");
                    break;
                }
                if (chunkId >= lastChunkInfo.chunkId)
                {
                    // auto incrementし続けるとkeyFrameIdだけ先行して404になるので調整
                    if (keyFrameId > lastChunkInfo.keyFrameId)
                    {
                        keyFrameId = lastChunkInfo.keyFrameId;
                    }
                    Thread.Sleep(lastChunkInfo.nextAvailableChunk);
                }
            }
        }
    }
}
