using System.Collections.Generic;

namespace Laserforce
{
    public class Session
    {
        public Session()
        {
            Playing = new List<Player>();
        }

        public List<Player> Playing { get; set; }
    }
}
