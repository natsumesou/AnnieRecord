using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace AnnieRecord
{
    public partial class Summoner : BaseModel
    {
        private static Summoner parse(IRestResponse response)
        {
            return new Summoner();
        }
    }
}
