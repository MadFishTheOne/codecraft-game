using System;

namespace MiniGame
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (MiniGame game = new MiniGame())
            {
                game.Run();
            }
        }
    }
}

