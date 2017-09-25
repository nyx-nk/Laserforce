using System.Threading;

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
                        // TODO: Add IronPython
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