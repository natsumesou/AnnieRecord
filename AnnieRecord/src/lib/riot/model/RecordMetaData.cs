using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord.riot.model
{
    class RecordMetaData
    {
        public long championId;
        public Game game;
        public DateTime gameStartTime;
        public bool winThisGame;
        public String encryptionKey;
    }
}
