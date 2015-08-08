using System;

namespace tilt
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (TiltMain game = new TiltMain())
            {
                game.Run();
            }
        }
    }
}
