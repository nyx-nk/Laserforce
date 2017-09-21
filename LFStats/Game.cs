namespace LFStats
{
    public class Game
    {
        public int Id { get; set; }
        public int RedScore { get; set; }
        public int RedAdjust { get; set; }
        public int GreenScore { get; set; }
        public int GreenAdjust { get; set; }
        public string Winner { get; set; }
        public bool RedEliminated { get; set; }
        public bool GreenEliminated { get; set; }
    }
}
