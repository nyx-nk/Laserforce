using System.Collections.Generic;

namespace LocalStats
{
    public class SingleGame
    {
        public int Id { get; set; }
        public int TotalRedScore { get; set; }
        public int TotalGreenScore { get; set; }
        public List<GamePlayer> RedTeam { get; set; }
        public List<GamePlayer> GreenTeam { get; set; }
    }
}