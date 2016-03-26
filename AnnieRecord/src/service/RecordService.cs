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

        public RecordService(Summoner s)
        {
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
            game = GameClient.createGameFromClientInfo(Riot.Instance.region);
            if (Replay.isExist(game))
                return;
            var replay = Replay.create(game);

            thread = new Thread(() => startRecord(replay));
            thread.IsBackground = true;
            thread.Start();
        }

        private void findAndPrepareGameInfo()
        {
            game = Game.findCurrent(summoner);
            System.Diagnostics.Debug.WriteLine(game.encryptionKey);

            var replay = Replay.create(game);

            thread = new Thread(() => startRecord(replay));
            thread.IsBackground = true;
            thread.Start();
        }

        private void startRecord(Replay replay)
        {
            Thread.Sleep(5000);

            System.Diagnostics.Debug.WriteLine("start recording");
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
