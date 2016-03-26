using AnnieRecord.riot;
using AnnieRecord.riot.model;
using System;
using System.Collections.Generic;
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
        private RecordService record;

        public MainWindow()
        {
            InitializeComponent();
            server = new Server();
            //startWatch();
        }

        private void startWatch()
        {
            var summonerName = summonerNameTextBox.Text;
            Riot.Instance.buildClient(new Region(Region.Type.jp), API_KEY);

            var summoner = Summoner.find(summonerName);
            System.Diagnostics.Debug.WriteLine(summoner.id);

            record = new RecordService(summoner, loldir, REPLAY_DIR);
            record.watchLocalAnnieRecordFile();
        }

        private void record_by_current_game(object sender, RoutedEventArgs e)
        {
            Riot.Instance.buildClient(new Region(Region.Type.na), API_KEY);

            var summonerName = summonerNameTextBox.Text;
            var summoner = Summoner.find(summonerName);

            record = new RecordService(summoner, loldir, REPLAY_DIR);
            record.startRecord();
        }

        private void launch_replay(object sender, RoutedEventArgs e)
        {
            var filename = summonerNameTextBox.Text;
            var replay = Replay.find(REPLAY_DIR, filename);

            server.run(replay);
            GameClient.LaunchReplay(replay, loldir);
        }
    }
}
