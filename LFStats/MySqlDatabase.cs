using System.Collections.Generic;
using System.Configuration;
using MySql.Data.MySqlClient;


namespace LFStats
{
    public class MySqlDatabase
    {
        #region Sql

        private const string _getGamesByCenterSql = @"
SELECT id, red_score, green_score, red_adj, green_adj, winner, red_eliminated, green_eliminated
FROM lfstats.games
WHERE center_id = {0}";

        private const string _getPlayersSql = @"
SELECT id, player_name
FROM lfstats.players";

        private const string _getAllPlayerGameScoresByCenterSql = @"
SELECT id, player_name, team, position, shots_hit, shots_fired, times_zapped, times_missiled, missile_hits, nukes_activated,
nukes_detonated, nukes_canceled, medic_hits, own_medic_hits, medic_nukes, scout_rapid, life_boost, ammo_boost, lives_left, score,
shots_left, penalties, shot_3hit, elim_other_team, team_elim, own_nuke_cancels, shot_opponent, shot_team, missiled_opponent, 
missiled_team, resupplies, rank, bases_destroyed, accuracy, mvp_points, sp_earned, sp_spent, game_id, player_id
FROM lfstats.scorecards
WHERE center_id = {0}";

        private const string _getPlayerGameScoresByPlayerSql = @"
SELECT id, player_name, team, position, shots_hit, shots_fired, times_zapped, times_missiled, missile_hits, nukes_activated,
nukes_detonated, nukes_canceled, medic_hits, own_medic_hits, medic_nukes, scout_rapid, life_boost, ammo_boost, lives_left, score,
shots_left, penalties, shot_3hit, elim_other_team, team_elim, own_nuke_cancels, shot_opponent, shot_team, missiled_opponent, 
missiled_team, resupplies, rank, bases_destroyed, accuracy, mvp_points, sp_earned, sp_spent, game_id, player_id
FROM lfstats.scorecards
WHERE center_id = {0}
    AND player_id = {1}";

        #endregion Sql

        #region Constructors

        public MySqlDatabase()
        {
            Server = ConfigurationManager.AppSettings["server"];
            Database = ConfigurationManager.AppSettings["database"];
            Username = ConfigurationManager.AppSettings["username"];
            Password = ConfigurationManager.AppSettings["password"];
        }

        public MySqlDatabase(string server, string database, string username, string password)
        {
            Server = server;
            Database = database;
            Username = username;
            Password = password;
        }

        #endregion Constructors

        #region Properties

        public string Server { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConnectionString
        {
            get { return $"Server={Server}; database={Database}; UID={Username}; Password={Password}"; }
        }
        public MySqlConnection Connection { get; private set; }

        #endregion Properties

        #region Methods

        public void OpenConnection()
        {
            if (Connection == null)
            {
                Connection = new MySqlConnection(ConnectionString);
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

        public List<Game> GetGames(int centerId)
        {
            if (Connection == null)
            {
                OpenConnection();
            }

            var query = string.Format(_getGamesByCenterSql, centerId);

            var command = new MySqlCommand(query, Connection);

            var games = new List<Game>();

            try
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        games.Add(new Game()
                        {
                            Id = reader.GetInt32("id"),
                            RedScore = reader.GetInt32("red_score"),
                            GreenScore = reader.GetInt32("green_score"),
                            RedAdjust = reader.GetInt32("red_adj"),
                            GreenAdjust = reader.GetInt32("green_adj"),
                            Winner = reader.GetString("winner"),
                            RedEliminated = reader.GetBoolean("red_eliminated"),
                            GreenEliminated = reader.GetBoolean("green_eliminated")
                        });
                    }
                }
            }
            catch (MySqlException mse)
            {
                // TODO: Handle this
            }

            return games;
        }

        public List<Player> GetAllPlayers()
        {
            if (Connection == null)
            {
                OpenConnection();
            }

            var command = new MySqlCommand(_getPlayersSql, Connection);

            var players = new List<Player>();

            try
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        players.Add(new Player()
                        {
                            Id = reader.GetInt32("id"),
                            Name = reader.GetString("player_name")
                        });
                    }
                }
            }
            catch (MySqlException mse)
            {
                // TODO: Handle this
            }

            return players;
        }

        public List<PlayerGameScore> GetAllPlayerGameScores(int centerId)
        {
            return GetPlayerGameScores(centerId, null);
        }

        public List<PlayerGameScore> GetSpecificPlayerGameScores(int centerId, int playerId)
        {
            return GetPlayerGameScores(centerId, playerId);
        }

        #endregion Methods

        #region Private Methods

        private List<PlayerGameScore> GetPlayerGameScores(int centerId, int? playerId)
        {
            if (Connection == null)
            {
                OpenConnection();
            }

            string query = string.Empty;

            if (playerId.HasValue)
            {
                query = string.Format(_getPlayerGameScoresByPlayerSql, centerId, playerId);
            }
            else
            {
                query = string.Format(_getAllPlayerGameScoresByCenterSql, centerId);
            }

            var command = new MySqlCommand(query, Connection);

            var playerGameScores = new List<PlayerGameScore>();

            try
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        playerGameScores.Add(new PlayerGameScore()
                        {
                            Id = reader.GetInt32("id"),
                            PlayerName = reader.GetString("player_name"),
                            Team = reader.GetString("team"),
                            Position = reader.GetString("position"),
                            ShotsHit = reader.GetInt32("shots_hit"),
                            ShotsFired = reader.GetInt32("shots_fired"),
                            TimesZapped = reader.GetInt32("times_zapped"),
                            TimesMissiled = reader.GetInt32("times_missiled"),
                            MissileHits = reader.GetInt32("missile_hits"),
                            NukesActivated = reader.GetInt32("nukes_activated"),
                            NukesDetonated = reader.GetInt32("nukes_detonated"),
                            NukesCancelled = reader.GetInt32("nukes_canceled"),
                            MedicHits = reader.GetInt32("medic_hits"),
                            OwnMedicHits = reader.GetInt32("own_medic_hits"),
                            MedicNukes = reader.GetInt32("medic_nukes"),
                            ScoutRapidFires = reader.GetInt32("scout_rapid"),
                            LivesBoosts = reader.GetInt32("life_boost"),
                            AmmoBoosts = reader.GetInt32("ammo_boost"),
                            LivesLeft = reader.GetInt32("lives_left"),
                            Score = reader.GetInt32("score"),
                            ShotsLeft = reader.GetInt32("shots_left"),
                            Penalties = reader.GetInt32("penalties"),
                            ShotThreeHit = reader.GetInt32("shot_3hit"),
                            ElimOtherTeam = reader.GetBoolean("elim_other_team"),
                            ElimTeam = reader.GetBoolean("team_elim"),
                            OwnNukeCancels = reader.GetInt32("own_nuke_cancels"),
                            ShotOpponents = reader.GetInt32("shot_opponent"),
                            ShotTeam = reader.GetInt32("shot_team"),
                            MissiledOpponent = reader.GetInt32("missiled_opponent"),
                            MissiledTeam = reader.GetInt32("missiled_team"),
                            Resupplies = reader.GetInt32("resupplies"),
                            Rank = reader.GetInt32("rank"),
                            BasesDestroyed = reader.GetInt32("bases_destroyed"),
                            Accuracy = reader.GetDecimal("accuracy"),
                            MvpPoints = reader.GetDecimal("mvp_points"),
                            SpecialEarned = reader.GetInt32("sp_earned"),
                            SpecialSpent = reader.GetInt32("sp_spent"),
                            GameId = reader.GetInt32("game_id"),
                            PlayerId = reader.GetInt32("player_id")
                        });
                    }
                }
            }
            catch (MySqlException mse)
            {
                // TODO: Handle this
                var breakHere = true;
            }

            return playerGameScores;
        }

        #endregion Private Methods
    }
}
