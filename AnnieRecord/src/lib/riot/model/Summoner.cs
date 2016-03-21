using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord
{
    public partial class Summoner : BaseModel
    {
        public String name {
            get;
            private set;
        }
        public uint id
        {
            get;
            private set;
        }

        public Summoner()
        {
            
        }
        public Summoner(String name, uint id)
        {
            this.name = name;
            this.id = id;
        }
    }
}
