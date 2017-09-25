using AKLStats;
using LFStats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LF
{
    public class Program
    {
        static void Main(string[] args)
        {
            //TestStatsDatabase();
            //TestLocalDatabase();

            var synchroniser = new StatsSynchroniser();
            //var synchroniser = new StatsSynchroniser(16);
            synchroniser.Update();

            Console.Write("Done");

            Console.ReadKey();
        }


        #region Test Methods

        private static void TestStatsDatabase()
        {
            var statsDatabase = new MySqlDatabase();
            statsDatabase.OpenConnection();

            var games = statsDatabase.GetCenterGames(16);
            var players = statsDatabase.GetAllPlayers();
            var playerGameScores = statsDatabase.GetPlayerGameScoresForCenter(16);

            var dmihawk = players.FirstOrDefault(x => x.Name.ToLower() == "dmihawk");

            var myScores = new List<PlayerGameScore>();

            if (dmihawk != null)
            {
                myScores = statsDatabase.GetPlayerGameScoresForPlayer(16, dmihawk.Id);
            }

            statsDatabase.CloseConnection();
        }

        private static void TestLocalDatabase()
        {
            var localDatabase = new SqliteDatabase(false);
            localDatabase.Path = "..\\..\\stats.db";

            localDatabase.OpenConnection();

            var testPlayers = new List<Player>();
            testPlayers.Add(new Player() { Id = 1, Name = "Test" });

            localDatabase.AddPlayers(testPlayers);

            localDatabase.CloseConnection();
        }

        #endregion Test Methods
    }
}
