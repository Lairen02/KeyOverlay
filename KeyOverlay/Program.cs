using System;
using System.IO;
using System.Linq;
using System.Security.Principal;

namespace KeyOverlay
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            AppWindow window;
            try
            {
                window = new AppWindow(args.FirstOrDefault());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                using var sw = new StreamWriter("errorMessage.txt");
                sw.WriteLine(e);
                throw;
            }
            window.Run();
            
        }
    }
}
