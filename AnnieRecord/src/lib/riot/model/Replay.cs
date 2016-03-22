using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord
{
    public partial class Replay : BaseModel
    {
        public long gameId;
        public String version;
        public String encryptionKey;
        public String metaData;
        public DateTime gameStartTime;
        public Dictionary<int, String> chunks;
        public Dictionary<int, String> keyFrames;

        public Replay(long id, String versionStr, String encryptionKeyStr, String metaDataStr, DateTime startTime)
        {
            gameId = id;
            version = versionStr;
            encryptionKey = encryptionKeyStr;
            metaData = metaDataStr;
            gameStartTime = startTime;
        }

        public String fileName()
        {
            var time = gameStartTime.ToString("yyyyMMdd-HHmmss");
            return String.Format("replay_{0}.anr", time);
        }
    }
}
