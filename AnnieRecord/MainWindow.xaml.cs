using AnnieRecord.riot;
using AnnieRecord.riot.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AnnieRecord
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Server server;
        private readonly String API_KEY = "9655dc94-8557-43e7-9927-6606c68beb30";
        private static readonly String REPLAY_DIR = Environment.CurrentDirectory + "\\replays";
        private String loldir = @"C:\Riot Games";
        private Record record;

        public MainWindow()
        {
            InitializeComponent();
            server = new Server();

            var files = Directory.GetFiles(REPLAY_DIR);
            foreach(var file in files)
            {
                if (!file.Contains(Replay.TEMP_FILENAME))
                {
                    var replay = Replay.find(REPLAY_DIR, System.IO.Path.GetFileName(file));
                    System.Diagnostics.Debug.WriteLine(replay.game.won);
                }
            }

            //startWatch();
        }

        private void startWatch()
        {
            var summonerName = summonerNameTextBox.Text;
            Riot.Instance.buildClient(new Region(Region.Type.jp), API_KEY);

            var summoner = Summoner.find(summonerName);
            System.Diagnostics.Debug.WriteLine(summoner.id);

            record = new Record(summoner, loldir, REPLAY_DIR);
            record.watchLocalAnnieRecordFile();
        }

        private void record_by_current_game(object sender, RoutedEventArgs e)
        {
            Riot.Instance.buildClient(new Region(Region.Type.kr), API_KEY);

            var summonerName = summonerNameTextBox.Text;
            var summoner = Summoner.find(summonerName);
            if (summoner == null)
            {
                System.Diagnostics.Debug.WriteLine("Summoner Not Found: " + summonerName);
            }
            else
            {
                record = new Record(summoner, loldir, REPLAY_DIR);
                record.startRecord();
            }
        }

        private void launch_replay(object sender, RoutedEventArgs e)
        {
            var filename = summonerNameTextBox.Text;
            var replay = Replay.find(REPLAY_DIR, filename);
            replay.loadPlayData();

            server.run(replay);
            GameClient.LaunchReplay(replay, server, loldir);
        }

        private void abort_record(object sender, RoutedEventArgs e)
        {
            if(record.isRecording())
                record.abort();
        }
    }
}
