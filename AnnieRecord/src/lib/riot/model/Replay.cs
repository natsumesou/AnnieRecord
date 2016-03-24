using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnnieRecord
{
    public partial class Replay : BaseModel
    {
        public long gameId;
        public Region region;
        public String encryptionKey;
        public Dictionary<String, byte[]> data;

        public String lastChunkPath;
        public String lastkeyFramePath;

        private static readonly String FILENAME_FORMAT = "replay_{0}_{1}.anr";

        public String filename
        {
            get
            {
                return String.Format(FILENAME_FORMAT, gameId, region.platform);
            }
        }

        public static String filenameFromGame(Game game)
        {
            return String.Format(FILENAME_FORMAT, game.id, game.platformId);
        }

        public Replay(long id, String encryptionKeyStr, Region reg)
        {
            gameId = id;
            encryptionKey = encryptionKeyStr;
            region = reg;
            data = new Dictionary<string, byte[]>();
        }

        public void buildEncryptionKey(String key)
        {
            encryptionKey = key;
        }

        public byte[] getData(HttpListenerRequest request)
        {
            var key = String.Format("{0} {1}", request.HttpMethod, request.RawUrl);
            return data[key];
        }
    }
}
