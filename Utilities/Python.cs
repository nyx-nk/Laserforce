using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class Python
    {
        public static void ExecuteScript(string path)
        {
            try
            {
                var process = new Process();
                process.StartInfo = new ProcessStartInfo(@"C:\Python27\python.exe", path)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                process.Start();

                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                Console.WriteLine(output);

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }
    }
}