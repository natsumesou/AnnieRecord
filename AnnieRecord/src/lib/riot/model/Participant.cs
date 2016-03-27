using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord.riot.model
{
    [Serializable]
    public class Participant : BaseModel
    {
        public enum TEAM { BLUE, PURPLE }

        [JsonProperty("summonerName")]
        public String summonerName
        {
            get;
            private set;
        }
        [JsonProperty("championId")]
        public long championId
        {
            get;
            private set;
        }
        [JsonProperty("teamId")]
        public long teamId
        {
            get;
            private set;
        }
        [JsonProperty("summonerId")]
        public long summonerId
        {
            get;
            private set;
        }
        [JsonProperty("spell1Id")]
        public long spell1Id
        {
            get;
            private set;
        }
        [JsonProperty("spell2Id")]
        public long spell2Id
        {
            get;
            private set;
        }

        [JsonProperty("_team")]
        private TEAM? _team;
        public TEAM? team
        {
            get
            {
                if (_team == null)
                {
                    _team = teamId == 100 ? TEAM.BLUE : TEAM.PURPLE;
                }
                return _team;
            }
        }
    }
}
