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
        public MainWindow()
        {
            InitializeComponent();
            startWatch();
        }

        private void startWatch()
        {
            var summonerName = summonerNameTextBox.Text;
            var riot = new Riot(Region.Type.jp);

            var summoner = riot.findSummoner(summonerName);
            System.Diagnostics.Debug.WriteLine(summoner.id);
            var record = new RecordService(riot, summoner);
            record.watch();
        }

        private void launch_replay(object sender, RoutedEventArgs e)
        {
            var filename = summonerNameTextBox.Text;
            var riot = new Riot(Region.Type.jp);
            var replay = riot.findReplay(filename);
            var server = new Server(replay);
            server.run();
            Client.LaunchReplay(replay);
        }
    }
}
