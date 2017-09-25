using System.IO;
using System.Text;
using Newtonsoft.Json;
using System;

namespace Laserforce
{ 
    public static class JsonWriter
    {
        public static bool WritePlayerFile(Player player)
        {
            var result = false;

            var playerTag = player.Tag;

            if (playerTag == "*******")
            {
                playerTag = "Chen";
            }

            var path = $"{Directory.GetCurrentDirectory()}\\PlayerData\\{player.Number};{playerTag}.txt";

            if (!Directory.Exists($"{Directory.GetCurrentDirectory()}\\PlayerData"))
            {
                Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}\\PlayerData");
            }

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                var json = JsonConvert.SerializeObject(player);

                using (var filestream = File.CreateText(path))
                {
                    filestream.Write(json);
                }

                result = true;
            }
            catch (Exception ex)
            {
                var t = ex;
            }

            return result;
        }

        public static Player ReadPlayerFile(string fileName)
        {
            var path = $"{Directory.GetCurrentDirectory()}\\PlayerData\\{fileName}.txt";

            if (!File.Exists(path)) throw new FileNotFoundException();

            var text = File.ReadAllText(path);

            return JsonConvert.DeserializeObject<Player>(text);
        }
    }
}
