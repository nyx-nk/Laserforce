namespace Laserforce
{
    public class GamePlayer
    {
        public Player Player { get; set; }
        public Team Team { get; set; }
        public int Role { get; set; }
    }

    public enum Team
    {
        Red,
        Green,
        Unassigned
    }
}
