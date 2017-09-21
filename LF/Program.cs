using LFStats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LF
{
    class Program
    {
        static void Main(string[] args)
        {
            var statsDatabase = new MySqlDatabase();
            statsDatabase.OpenConnection();

            var games = statsDatabase.GetGames(16);
            var players = statsDatabase.GetAllPlayers();
            var playerGameScores = statsDatabase.GetAllPlayerGameScores(16);

            var dmihawk = players.FirstOrDefault(x => x.Name.ToLower() == "dmihawk");

            var myScores = new List<PlayerGameScore>();

            if (dmihawk != null)
            {
                myScores = statsDatabase.GetSpecificPlayerGameScores(16, dmihawk.Id);
            }

            Console.ReadKey();
        }
    }
}
