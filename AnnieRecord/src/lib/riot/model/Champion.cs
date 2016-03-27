using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord.riot.model
{
    [Serializable]
    public partial class Champion : BaseModel
    {
        public long id
        {
            get;
            private set;
        }
        public String name
        {
            get;
            private set;
        }
        public String key
        {
            get;
            private set;
        }
    }
}
