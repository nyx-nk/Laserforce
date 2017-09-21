using MySql.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace LFStats
{
    public class MySqlDatabase
    {
        public MySqlDatabase(string server, string database, string username, string password)
        {
            Server = server;
            Database = database;
            Username = username;
            Password = password;
        }

        public string Server { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConnectionString
        {
            get { return $"Server={Server}; database={Database}; UID={Username}; Password={Password}"; }
        }
        public MySqlConnection Connection { get; private set; }

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

        public List<Game> GetGames()
        {
            if (Connection == null)
            {
                OpenConnection();
            }

            var query = @"
SELECT id, red_score, green_score, red_adj, green_adj, winner, red_eliminated, green_eliminated
FROM lfstats.games
WHERE center_id = 16";

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
                // Uh-oh!
            }

            return games;
        }
    }
}
