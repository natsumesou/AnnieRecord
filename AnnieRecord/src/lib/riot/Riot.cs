using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace AnnieRecord
{
    public class Riot
    {
        public Riot(Region.Type regionType)
        {
            var region = new Region(regionType);
            API.Instance.buildClient(region);
        }

        public Summoner findSummoner(String summonerName)
        {
            return Summoner.find(summonerName);
        }

        public Game findCurrentGame(Summoner summoner)
        {
            return Game.find(summoner);
        }

        public Replay findReplay(Game game)
        {
            return Replay.find(game);
        }
        
        public Replay findReplay(String filename)
        {
            return Replay.find(filename);
        }

        public LastChunkInfo findLastChunkInfo(Game game)
        {
            return LastChunkInfo.find(game);
        }
    }
}
