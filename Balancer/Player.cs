using System.Collections.Generic;

namespace Laserforce
{
    public class Player
    {
        public Player()
        {
        }

        public int Number { get; set; }
        public string RealName { get; set; }
        public string Tag { get; set; }

        public decimal OverallRank { get; set; }

        public decimal CommanderRank { get; set; }
        public decimal HeavyRank { get; set; }
        public decimal ScoutRank { get; set; }
        public decimal AmmoRank { get; set; }
        public decimal MedicRank { get; set; }
    }
}
