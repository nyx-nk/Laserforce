using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Laserforce
{
    public class Admin
    {
        public Admin()
        {

        }

        public void Initialise()
        {
            LoadAllPlayers();
        }

        public Player GetPlayer(int id)
        {
            return Players.FirstOrDefault(x => x.Number == id);
        }

        public Player GetPlayer(string name)
        {
            return Players.FirstOrDefault(x => x.Tag == name);
        }

        public List<Player> Players { get; set; }

        public Player CurrentPlayer { get; set; }

        private void LoadAllPlayers()
        {
            Players = new List<Player>();

            var path = $"{Directory.GetCurrentDirectory()}\\PlayerData";

            if (Directory.Exists(path))
            {
                var errors = new StringBuilder();

                foreach (var playerFile in Directory.GetFiles(path))
                {
                    var fileName = Path.GetFileNameWithoutExtension(playerFile);

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        if (fileName != "Template")
                        {
                            Players.Add(JsonWriter.ReadPlayerFile(fileName));
                        }
                    }
                    else
                    {
                        errors.AppendLine(fileName);
                    }
                }

                if (!string.IsNullOrEmpty(errors.ToString()))
                {
                    MessageBox.Show($"The following player data files are invalid: \n{errors.ToString()}");
                }
            }
            else
            {
                MessageBox.Show("The player data folder is missing");
            }

            Players = Players.OrderBy(x => x.Tag).ToList();
        }
    }
}
