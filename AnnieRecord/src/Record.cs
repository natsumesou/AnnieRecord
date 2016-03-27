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
    public class Record
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
        public Record(Summoner s, String clientDirectory, String recordDirectory)
        {
            this.summoner = s;
            this.recordDir = recordDirectory;
            this.clientDir = clientDirectory;
        }

        /// <summary>
        /// LoLのクライアントが起動したら自動で録画処理を行う
        /// API軽油でゲーム情報を取得する
        /// </summary>
        public void watch()
        {
            ManagementEventWatcher startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            startWatch.EventArrived += new EventArrivedEventHandler(startWatch_EventArrived);
            startWatch.Start();
        }

        /// <summary>
        /// 途中でも録画を終了する
        /// 録画中のファイルは削除される
        /// </summary>
        public void abort()
        {
            thread.Abort();
            if (isRecording())
            {
                replay.abort();
            }
        }

        /// <summary>
        /// LoLのクライアントが起動したら児童で録画処理を行う
        /// ローカルのAnnieRecrdファイルからゲームデータを取得する
        /// </summary>
        [Obsolete]
        public void watchLocalAnnieRecordFile() {
            ManagementEventWatcher startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            startWatch.EventArrived += new EventArrivedEventHandler(startWatch_EventArrived_Local);
            startWatch.Start();
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
        public bool isRecording()
        {
            if (replay == null)
                return false;
            if (thread == null)
                return false;
            return thread.IsAlive && replay.isWriting();
        }

        private void startWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (!e.NewEvent.Properties["ProcessName"].Value.Equals(GameClient.CLIENT_NAME))
                return;
            findAndPrepareGameInfo();
        }

        [Obsolete]
        private void startWatch_EventArrived_Local(object sender, EventArrivedEventArgs e)
        {
            if (!e.NewEvent.Properties["ProcessName"].Value.Equals(GameClient.CLIENT_NAME))
                return;
            findAndPrepareGameInfoFromLocal();
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
            if(game == null)
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
            System.Diagnostics.Debug.WriteLine("start recording: " + r.game.player.summonerName);
            replay = r;
            int chunkId = 1;
            int keyFrameId = 1;
            while(true)
            {
                replay.writeChunk(chunkId++);
                replay.writeKeyFrame(keyFrameId++);
                System.Diagnostics.Debug.WriteLine("writing chunk: " + chunkId);

                var lastChunkInfo = LastChunkInfo.find(game);

                if (lastChunkInfo.isLastChunk())
                {
                    replay.writeChunk(lastChunkInfo.chunkId);
                    var match = Game.findMatch(game);
                    game.buildMatchData(match);
                    replay.exportANRFile(game);
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
