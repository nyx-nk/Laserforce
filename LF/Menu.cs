using System;

namespace LF
{
    public class Menu
    {
        public Menu()
        {

        }

        public MenuSelection ShowMainMenu()
        {
            DisplayOptions();

            var input = Console.ReadKey();

            switch (input.Key)
            {
                case ConsoleKey.Oem1:
                    return MenuSelection.SyncLocal;

                case ConsoleKey.Oem2:
                    return MenuSelection.SyncGlobal;

                case ConsoleKey.Oem3:
                    return MenuSelection.ExecSvm;

                case ConsoleKey.Oem4:
                    return MenuSelection.Balancer;

                default:
                    return MenuSelection.None;
            }
        }

        private void DisplayOptions()
        {
            Console.Clear();

            Console.WriteLine("(1) Sync local stats");
            Console.WriteLine("(2) Sync global stats");
            Console.WriteLine("(3) Execute SVM analysis");
            Console.WriteLine("(4) Run Balancer");
        }
    }

    public enum MenuSelection
    {
        None,
        SyncLocal,
        SyncGlobal,
        ExecSvm,
        Balancer
    }
}
