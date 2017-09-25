using System.Threading;
using Utilities;

namespace LF
{
    public class Program
    {
        static void Main(string[] args)
        {
            var menu = new Menu();

            // yeah yeah... it's an infinite loop
            for (;;)
            {
                var option = menu.ShowMainMenu();

                switch (option)
                {
                    case MenuSelection.SyncLocal:
                        var syncLocal = new StatsSynchroniser(16);
                        syncLocal.Update();
                        break;

                    case MenuSelection.SyncGlobal:
                        var syncGlobal = new StatsSynchroniser();
                        syncGlobal.Update();
                        break;

                    case MenuSelection.ExecSvm:
                        Python.ExecuteScript(@"C:\Users\GHolmes\Documents\Visual Studio 2015\Projects\LF\Laserforce\Analysis\svm_analyse.py");
                        break;

                    case MenuSelection.Balancer:
                        // TODO: Fire up program
                        break;
                }

                Thread.Sleep(1000);
            }
        }
    }
}