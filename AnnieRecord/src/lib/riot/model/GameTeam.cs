using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord.riot.model
{
    public class GameTeam : BaseModel
    {
        public long teamId
        {
            get;
            private set;
        }
        public bool winner
        {
            get;
            private set;
        }
    }
}
