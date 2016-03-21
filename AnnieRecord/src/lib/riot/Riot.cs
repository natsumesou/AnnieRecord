using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace AnnieRecord
{
    class Riot
    {
        public static Summoner findSummoner(String summonerName)
        {
            return Summoner.find(summonerName);
        }
    }
}
