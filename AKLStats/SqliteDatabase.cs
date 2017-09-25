using Laserforce;
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

        private const string _getAllPlayerStatsForGameSql = @"
SELECT pgs.Id, pgs.PlayerName, pgs.Team, pgs.Position, pgs.ShotsHit, pgs.ShotsFired, pgs.TimesZapped, pgs.TimesMissiled, pgs.MissileHits,
    pgs.NukesActivated, pgs.NukesDetonated, pgs.NukeCancelled, pgs.MedicHits, pgs.OwnMedicHits, pgs.MedicNukes, pgs.ScoutRapidFires,
    pgs.LivesBoosts, pgs.AmmoBoosts, pgs.LivesLeft, pgs.Score, pgs.Penalties, pgs.ShotThreeHit, pgs.ElimOtherTeam, pgs.ElimTeam,
    pgs.OwnNukeCancels, pgs.ShotOpponents, pgs.ShotTeam, pgs.MissiledOpponent, pgs.MissiledTeam, pgs.Resupplies, pgs.Rank,
    pgs.BasesDestroyed, pgs.Accuracy, pgs.MvpPoints, pgs.SpecialEarned, pgs.SpecialSpent
FROM PlayerGameScore pgs
WHERE pgs.GameId = {0}";

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

        public SingleGame GetGame(int id)
        {
            if (Connection == null)
            {
                Connection.Open();
            }

            var query = string.Format(_getAllPlayerStatsForGameSql, id);

            var game = new SingleGame();

            try
            {
                var command = new SQLiteCommand(query, Connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var player = new GamePlayer();
                        player.Id = (int)reader["Id"];
                        player.Name = reader["PlayerName"].ToString();
                        player.Position = ConvertPositionToRole(reader["Position"].ToString());
                        player.ShotsHit = (int)reader["ShotsHit"];
                        player.ShotsFired = (int)reader["ShotsFired"];
                        player.TimesZapped = (int)reader["TimesZapped"];
                        player.TimesMissiled = (int)reader["TimesMissiled"];
                        player.MissileHits = (int)reader["MissileHits"];
                        player.NukesActivated = (int)reader["NukesActivated"];
                        player.NukesDetonated = (int)reader["NukesDetonated"];
                        player.NukesCancelled = (int)reader["NukeCancelled"];
                        player.MedicHits = (int)reader["MedicHits"];
                        player.OwnMedicHits = (int)reader["OwnMedicHits"];
                        player.MedicNukes = (int)reader["MedicNukes"];
                        player.ScoutRapidFires = (int)reader["ScoutRapidFires"];
                        player.LivesBoosts = (int)reader["LivesBoosts"];
                        player.AmmoBoosts = (int)reader["AmmoBoosts"];
                        player.LivesLeft = (int)reader["LivesLeft"];
                        player.Score = (int)reader["Score"];
                        player.Penalties = (int)reader["Penalties"];
                        player.ShotThreeHit = (int)reader["ShotThreeHit"];
                        player.ElimOtherTeam = (int)reader["ElimOtherTeam"];
                        player.ElimTeam = (int)reader["ElimTeam"];
                        player.OwnNukeCancels = (int)reader["OwnNukeCancels"];
                        player.ShotOpponents = (int)reader["ShotOpponents"];
                        player.ShotTeam = (int)reader["ShotTeam"];
                        player.MissiledOpponent = (int)reader["MissiledOpponent"];
                        player.MissiledTeam = (int)reader["MissileTeam"];
                        player.Resupplies = (int)reader["Resupplies"];
                        player.Rank = (int)reader["Rank"];
                        player.BasesDestroyed = (int)reader["BasesDestroyed"];
                        player.Accuracy = (decimal)reader["Accuracy"];
                        player.MvpPoints = (decimal)reader["MvpPoints"];
                        player.SpecialEarned = (int)reader["SpecialEarned"];
                        player.SpecialSpent = (int)reader["SpecialSpent"];

                        var team = reader["Team"].ToString().ToLower();

                        if (team == "red")
                        {
                            game.RedTeam.Add(player);
                        }
                        else if (team == "green")
                        {
                            game.GreenTeam.Add(player);
                        }
                        else
                        {
                            throw new InvalidOperationException($"Unknown team: {team}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return game;
        }

        #endregion Methods

        private SpaceMarines5Role ConvertPositionToRole(string position)
        {
            switch (position.ToLower())
            {
                case "commander":
                    return SpaceMarines5Role.Commander;

                case "heavy weapons":
                    return SpaceMarines5Role.Heavy;

                case "scout":
                    return SpaceMarines5Role.Scout;

                case "ammo carrier":
                    return SpaceMarines5Role.Ammo;

                case "medic":
                    return SpaceMarines5Role.Medic;

                default:
                    throw new InvalidOperationException($"Unknown position {position}");
            }
        }
    }
}