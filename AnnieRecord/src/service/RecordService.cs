using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord
{
    public class RecordService
    {
        public static void pooling(Riot riot)
        {
            System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcessesByName("League of Legends");

            foreach (System.Diagnostics.Process p in ps)
            {
                System.Diagnostics.Debug.WriteLine("{0}/{1}", p.Id, p.MainWindowTitle);
            }
        }

        private static void startRecord(Riot riot)
        {

        }
    }
}
