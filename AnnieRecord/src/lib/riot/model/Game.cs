using Newtonsoft.Json;
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
        [JsonProperty("id")]
        public long id
        {
            get;
            set;
        }
        [JsonProperty("mode")]
        public Mode mode
        {
            get;
            private set;
        }
        [JsonProperty("platformId")]
        private string platformId
        {
            get;
            set;
        }
        [JsonProperty("type")]
        public Type type
        {
            get;
            private set;
        }
        [JsonProperty("recordStartTime")]
        public DateTime recordStartTime
        {
            get;
            private set;
        }

        [DeserializeAs(Name = "observers.encryptionKey")]
        [JsonProperty("encryptionKey")]
        public String encryptionKey
        {
            get;
            private set;
        }
        [JsonProperty("participants")]
        public List<Participant> participants
        {
            get;
            private set;
        }
        [JsonProperty("player")]
        public Participant player
        {
            get;
            private set;
        }
        [JsonProperty("won")]
        public bool won
        {
            get;
            private set;
        }
        [JsonProperty("_region")]
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

        public Game()
        {
            recordStartTime = DateTime.Now;
        }

        public void buildMatchData(Match match)
        {
            this.won = match.won(player);
        }

        public static Game fromString(String str)
        {
            return JsonConvert.DeserializeObject<Game>(str);
        }

        [Obsolete]
        public static Game fromLocalClient(long gameId, String key, String platform)
        {
            return new Game() { id = gameId, encryptionKey = key, platformId = platform };
        }
        
        public String toJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}