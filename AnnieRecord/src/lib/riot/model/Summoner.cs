﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnieRecord.riot.model
{
    public partial class Summoner : BaseModel
    {
        public String name {
            get;
            private set;
        }
        public long id
        {
            get;
            private set;
        }
    }
}
