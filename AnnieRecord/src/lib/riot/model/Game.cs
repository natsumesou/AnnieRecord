using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord.riot.model
{
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

        public static Game fromLocalClient(long gameId, String key, String platform)
        {
            return new Game() { id = gameId, encryptionKey = key, platformId = platform };
        }
    }
}