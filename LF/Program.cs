using LFStats;
using System;

namespace LF
{
    class Program
    {
        static void Main(string[] args)
        {
            var statsDatabase = new MySqlDatabase("<server>", "<database>", "<username>", "<password>");
            statsDatabase.OpenConnection();

            var games = statsDatabase.GetGames();

            Console.ReadKey();
        }
    }
}
