using LocalStats;
using LFStats;
using System;
using System.Collections.Generic;
using Utilities;

namespace LF
{
    public class StatsSynchroniser
    {
        private MySqlDatabase _mySqlDatabase;
        private SqliteDatabase _sqliteDatabase;
        private bool _useGlobal;
        private int? _centerId;

        public StatsSynchroniser()
        {
            _mySqlDatabase = new MySqlDatabase();
            _sqliteDatabase = new SqliteDatabase(true);
            _useGlobal = true;
        }

        public StatsSynchroniser(int centerId)
        {
            _mySqlDatabase = new MySqlDatabase();
            _sqliteDatabase = new SqliteDatabase(false);
            _useGlobal = false;
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
                Logger.Error(ex.Message);

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
            var games = new List<Game>();
            if (_useGlobal)
            {
                games = _mySqlDatabase.GetAllGames();
            }
            else
            {
                games = _mySqlDatabase.GetCenterGames(_centerId.Value);
            }
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
            var playerGameScores = new List<PlayerGameScore>();
            if (_useGlobal)
            {
                playerGameScores = _mySqlDatabase.GetAllPlayerGameScores();
            }
            else
            {
                playerGameScores = _mySqlDatabase.GetPlayerGameScoresForCenter(_centerId.Value);
            }
            _mySqlDatabase.CloseConnection();

            // Save all the player game scores to the SQLite database
            _sqliteDatabase.OpenConnection();
            Console.WriteLine("Saving player game scores...");
            _sqliteDatabase.AddPlayerGameScores(playerGameScores);
            _sqliteDatabase.CloseConnection();
        }
    }
}
