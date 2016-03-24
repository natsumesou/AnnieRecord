using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Threading;

namespace AnnieRecord
{
    public class RecordService
    {
        private Riot riot;
        private Summoner summoner;
        private Game game;
        private Thread thread;

        public RecordService(Riot r, Summoner s)
        {
            this.riot = r;
            this.summoner = s;
        }

        public void watch()
        {
            ManagementEventWatcher startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            startWatch.EventArrived += new EventArrivedEventHandler(startWatch_EventArrived);
            startWatch.Start();

            ManagementEventWatcher stopWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
            stopWatch.EventArrived += new EventArrivedEventHandler(stopWatch_EventArrived);
            stopWatch.Start();
        }

        private void startWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (!e.NewEvent.Properties["ProcessName"].Value.Equals(Client.CLIENT_NAME))
                return;
            findAndPrepareGameInfoFromLocal();
        }

        private void stopWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (!e.NewEvent.Properties["ProcessName"].Value.Equals(Client.CLIENT_NAME))
                return;
        }

        private void findAndPrepareGameInfoFromLocal()
        {
            game = Client.createGameFromClientInfo();
            if (Replay.isExist(game))
                return;
            var replay = riot.findReplay(game);
            var lastChunkInfo = riot.findLastChunkInfo(game);

            thread = new Thread(() => startRecord(replay));
            thread.IsBackground = true;
            thread.Start();
        }

        private void findAndPrepareGameInfo()
        {
            game = riot.findCurrentGame(summoner);
            System.Diagnostics.Debug.WriteLine(game.encryptionKey);

            var replay = riot.findReplay(game);
            System.Diagnostics.Debug.WriteLine(replay.gameId);

            var lastChunkInfo =riot.findLastChunkInfo(game);
            System.Diagnostics.Debug.WriteLine(lastChunkInfo.chunkId);

            thread = new Thread(() => startRecord(replay));
            thread.IsBackground = true;
            thread.Start();
        }

        private void startRecord(Replay replay)
        {
            int chunkId = 1;
            int keyFrameId = 1;
            while(true)
            {
                replay.writeChunk(chunkId++);
                replay.writeKeyFrame(keyFrameId++);

                var lastChunkInfo = riot.findLastChunkInfo(game);

                if (lastChunkInfo.isLastChunk())
                {
                    replay.writeLastChunkInfo();
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
