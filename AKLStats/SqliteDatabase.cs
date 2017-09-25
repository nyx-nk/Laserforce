using LFStats;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using Utilities;

namespace LocalStats
{
    public class SqliteDatabase
    {
        #region Sql

        private const string _insertGameSql = @"
INSERT OR IGNORE INTO Game
(Id, RedScore, RedAdjust, GreenScore, GreenAdjust, Winner, RedEliminated, GreenEliminated)
VALUES ({0}, {1}, {2}, {3}, {4}, '{5}', {6}, {7})";

        private const string _insertPlayerSql = @"
INSERT OR IGNORE INTO Player
(Id, Name)
VALUES ({0}, '{1}')";

        private const string _insertPlayerGameScoreSql = @"
INSERT OR IGNORE INTO PlayerGameScore
(Id, PlayerName, Team, Position, ShotsHit, ShotsFired, TimesZapped, TimesMissiled, MissileHits, NukesActivated, NukesDetonated,
NukeCancelled, MedicHits, OwnMedicHits, MedicNukes, ScoutRapidFires, LivesBoosts, AmmoBoosts, LivesLeft, Score, Penalties,
ShotThreeHit, ElimOtherTeam, ElimTeam, OwnNukeCancels, ShotOpponents, ShotTeam, MissiledOpponent, MissiledTeam, Resupplies, Rank,
BasesDestroyed, Accuracy, MvpPoints, SpecialEarned, SpecialSpent, GameId, PlayerId)
VALUES ({0}, '{1}', '{2}', '{3}', {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20},
{21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29}, {30}, {31}, {32}, {33}, {34}, {35}, {36}, {37})";

        #endregion Sql

        #region Constructors

        public SqliteDatabase(bool useGlobal)
        {
            if (useGlobal)
            {
                Path = ConfigurationManager.AppSettings["GlobalSqlitePath"];
            }
            else
            {
                Path = ConfigurationManager.AppSettings["LocalSqlitePath"];
            }
        }

        public SqliteDatabase(string path)
        {
            Path = path;
        }

        #endregion Constructors

        #region Properties

        public string Path { get; set; }
        public string ConnectionString
        {
            get { return $@"Data Source={Path}; Version=3;"; }
        }
        public SQLiteConnection Connection { get; private set; }

        #endregion Properties

        #region Methods

        public void OpenConnection()
        {
            if (Connection == null)
            {
                Connection = new SQLiteConnection(ConnectionString);
            }

            Connection.Open();
        }

        public void CloseConnection()
        {
            if (Connection != null)
            {
                Connection.Close();
            }
        }

        public void AddGames(IList<Game> games)
        {
            if (Connection == null)
            {
                Connection.Open();
            }

            var query = string.Empty;

            foreach (var game in games)
            {
                // TODO: Guard this against SQL injections
                query = string.Format(_insertGameSql, game.Id, game.RedScore, game.RedAdjust, game.GreenScore,
                                        game.GreenAdjust, game.Winner, Convert.ToInt32(game.RedEliminated), 
                                        Convert.ToInt32(game.GreenEliminated));

                try
                {
                    var command = new SQLiteCommand(query, Connection);
                    command.ExecuteNonQuery();
                }
                catch (SQLiteException sle)
                {
                    // TODO: Handle this
                    Logger.Error(sle.Message);
                }
            }
        }

        public void AddPlayers(IList<Player> players)
        {
            if (Connection == null)
            {
                Connection.Open();
            }

            var query = string.Empty;

            foreach (var player in players)
            {
                // TODO: Guard this against SQL injections
                query = string.Format(_insertPlayerSql, player.Id, player.Name.ToSqlString());

                try
                {
                    var command = new SQLiteCommand(query, Connection);
                    command.ExecuteNonQuery();
                }
                catch (SQLiteException sle)
                {
                    // TODO: Handle this
                    Logger.Error(sle.Message);
                }
            }
        }

        public void AddPlayerGameScores(IList<PlayerGameScore> playerGameScores)
        {
            if (Connection == null)
            {
                Connection.Open();
            }

            var query = string.Empty;

            foreach (var pgs in playerGameScores)
            {
                // TODO: Guard this against SQL injections
                query = string.Format(_insertPlayerGameScoreSql, pgs.Id, pgs.PlayerName.ToSqlString(), pgs.Team, pgs.Position, pgs.ShotsHit,
                                        pgs.ShotsFired, pgs.TimesZapped, pgs.TimesMissiled, pgs.MissileHits, pgs.NukesActivated,
                                        pgs.NukesDetonated, pgs.NukesCancelled, pgs.MedicHits, pgs.OwnMedicHits, pgs.MedicNukes,
                                        pgs.ScoutRapidFires, pgs.LivesBoosts, pgs.AmmoBoosts, pgs.LivesLeft, pgs.Score, pgs.Penalties,
                                        pgs.ShotThreeHit, Convert.ToInt32(pgs.ElimOtherTeam), Convert.ToInt32(pgs.ElimTeam), 
                                        pgs.OwnNukeCancels, pgs.ShotOpponents, pgs.ShotTeam, pgs.MissiledOpponent, pgs.MissiledTeam, 
                                        pgs.Resupplies, pgs.Rank, pgs.BasesDestroyed, pgs.Accuracy, pgs.MvpPoints, pgs.SpecialEarned, 
                                        pgs.SpecialSpent, pgs.GameId, pgs.PlayerId);

                try
                {
                    var command = new SQLiteCommand(query, Connection);
                    command.ExecuteNonQuery();
                }
                catch (SQLiteException sle)
                {
                    // TODO: Handle this
                    Logger.Error(sle.Message);
                }
            }
        }

        #endregion Methods
    }
}