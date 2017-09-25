using System;
using System.Collections.Generic;
using System.Linq;

namespace Laserforce
{
    public class Game
    {      
        private Session _Session;
        private Random _Random;

        public Game(Session session)
        {
            _Random = new Random(DateTime.Now.Millisecond);
            _Session = session;
            Players = new List<GamePlayer>();
        }

        public GameMode GameMode { get; set; }
        public List<GamePlayer> Players { get; set; }

        public void RandomiseTeams(out List<Player> redTeam, out List<Player> greenTeam)
        {
            redTeam = new List<Player>();
            greenTeam = new List<Player>();

            var players = _Session.Playing.OrderBy(x => x.OverallRank);

            var redAverage = 0.0M;
            var greenAverage = 0.0M;

            for (int i = 0; i < players.Count(); ++i)
            {
                var player = players.ElementAt(i);

                if (redAverage > greenAverage)
                {
                    greenTeam.Add(player);
                    greenAverage += player.OverallRank;
                }
                else
                {
                    redTeam.Add(player);
                    redAverage += player.OverallRank;
                }
            }

            var bestRedTeam = new List<Player>();
            foreach (var player in redTeam)
            {
                bestRedTeam.Add(player);
            }
            var bestGreenTeam = new List<Player>();
            foreach (var player in greenTeam)
            {
                bestGreenTeam.Add(player);
            }

            var bestDifference = Math.Abs(bestRedTeam.Sum(x => x.OverallRank) - bestGreenTeam.Sum(x => x.OverallRank));

            redTeam.Clear();
            greenTeam.Clear();

            for (int i = 0; i < 1000; ++i)
            {
                foreach (var player in _Session.Playing)
                {
                    if (_Random.Next() % 2 == 0)
                    {
                        redTeam.Add(player);
                    }
                    else
                    {
                        greenTeam.Add(player);
                    }
                }

                var difference = Math.Abs(redTeam.Sum(x => x.OverallRank) - greenTeam.Sum(x => x.OverallRank));

                if (difference < bestDifference)
                {
                    bestRedTeam.Clear();

                    foreach (var player in redTeam)
                    {
                        bestRedTeam.Add(player);
                    }

                    bestGreenTeam.Clear();

                    foreach (var player in greenTeam)
                    {
                        bestGreenTeam.Add(player);
                    }

                    bestDifference = difference;
                }

                redTeam.Clear();
                greenTeam.Clear();
            }

            redTeam = bestRedTeam.OrderByDescending(x => x.OverallRank).ToList();
            greenTeam = bestGreenTeam.OrderByDescending(x => x.OverallRank).ToList();
        }
    }

    public enum GameMode
    {
        DungeonsAndDragons,
        SpaceMarines4,
        SpaceMarines5
    }
}
