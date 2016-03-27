using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord.riot.model
{
    [Serializable]
    public partial class Game : BaseModel
    {
        public enum Mode { CLASSIC, ODIN, ARAM, TUTORIAL, ONEFORALL, ASCENSION, FIRSTBLOOD, KINGPORO };
        public enum Type { CUSTOM_GAME, MATCHED_GAME, TUTORIAL_GAME };  

        [DeserializeAs(Name = "gameId")]
        public long id
        {
            get;
            private set;
        }
        public Mode mode
        {
            get;
            private set;
        }
        public string platformId
        {
            get;
            private set;
        }
        public Type type
        {
            get;
            private set;
        }
        [DeserializeAs(Name = "observers.encryptionKey")]
        public String encryptionKey
        {
            get;
            private set;
        }

        public List<Participant> participants
        {
            get;
            private set;
        }

        private Region _region;
        public Region region
        {
            get
            {
                if(_region == null)
                {
                    _region = Region.fromPlatformString(platformId);
                }
                return _region;
            }
        }

        public static Game fromBytes(byte[] bytes)
        {
            var bf = new BinaryFormatter();
            using(var ms = new MemoryStream(bytes))
            {
                return (Game)bf.Deserialize(ms);
            }
        }

        [Obsolete]
        public static Game fromLocalClient(long gameId, String key, String platform)
        {
            return new Game() { id = gameId, encryptionKey = key, platformId = platform };
        }

        public byte[] getBytes()
        {
            var bf = new BinaryFormatter();
            using(var ms = new MemoryStream())
            {
                bf.Serialize(ms, this);
                return ms.ToArray();
            }
        }
    }
}