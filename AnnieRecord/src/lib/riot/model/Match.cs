using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord.riot.model
{
    public partial class Match : BaseModel
    {
        public List<GameTeam> teams
        {
            get;
            private set;
        }

        public bool won(Participant participant)
        {
            var result = false;
            foreach(var team in teams)
            {
                if (team.teamId == participant.teamId)
                {
                    result = team.winner;
                }
            }
            return result;
        }
    }
}
