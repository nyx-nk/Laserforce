using AKLStats;
using LFStats;
using System;

namespace LF
{
    public class StatsSynchroniser
    {
        private MySqlDatabase _mySqlDatabase;
        private SqliteDatabase _sqliteDatabase;
        private int _centerId;

        public StatsSynchroniser(int centerId)
        {
            _mySqlDatabase = new MySqlDatabase();
            _sqliteDatabase = new SqliteDatabase();
            _centerId = centerId;
        }

        public bool Update()
        {
            try
            {
                UpdatePlayers();

                UpdateGames();

                UpdatePlayerGameScore();

                return true;
            }
            catch (Exception ex)
            {
                // TODO: Handle this

                return false;
            }
        }

        private void UpdatePlayers()
        {
            // Get all the players from the MySQL database
            _mySqlDatabase.OpenConnection();
            Console.WriteLine("Retrieving players...");
            var players = _mySqlDatabase.GetAllPlayers();
            _mySqlDatabase.CloseConnection();

            // Save all the players to the SQLite database
            _sqliteDatabase.OpenConnection();
            Console.WriteLine("Saving players...");
            _sqliteDatabase.AddPlayers(players);
            _sqliteDatabase.CloseConnection();
        }

        private void UpdateGames()
        {
            // Get all the games from the MySQL database
            _mySqlDatabase.OpenConnection();
            Console.WriteLine("Retrieving games...");
            var games = _mySqlDatabase.GetGames(_centerId);
            _mySqlDatabase.CloseConnection();

            // Save all the games to the SQLite database
            _sqliteDatabase.OpenConnection();
            Console.WriteLine("Saving games...");
            _sqliteDatabase.AddGames(games);
            _sqliteDatabase.CloseConnection();
        }

        private void UpdatePlayerGameScore()
        {
            // Get all the player game scores from the MySQL database
            _mySqlDatabase.OpenConnection();
            Console.WriteLine("Retrieving player game scores...");
            var playerGameScores = _mySqlDatabase.GetAllPlayerGameScores(_centerId);
            _mySqlDatabase.CloseConnection();

            // Save all the player game scores to the SQLite database
            _sqliteDatabase.OpenConnection();
            Console.WriteLine("Saving player game scores...");
            _sqliteDatabase.AddPlayerGameScores(playerGameScores);
            _sqliteDatabase.CloseConnection();
        }
    }
}
