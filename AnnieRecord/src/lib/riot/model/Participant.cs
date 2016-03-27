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

        public String summonerName
        {
            get;
            private set;
        }
        public long championId
        {
            get;
            private set;
        }
        public long teamId
        {
            get;
            private set;
        }
        public long summonerId
        {
            get;
            private set;
        }
        public long spell1Id
        {
            get;
            private set;
        }
        public long spell2Id
        {
            get;
            private set;
        }

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

        public bool isTarget(Summoner summoner)
        {
            return summonerId == summoner.id;
        }
    }
}
