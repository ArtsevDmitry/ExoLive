using System;
using ExoLive.Server.Common;
using ExoLive.Server.Core;

namespace ExoLive.Server.App
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("ExoLive server starting");
                Console.WriteLine("[1/2] - Init complete");
                ServerEngine.Instance.Start();
                Console.WriteLine("[2/2] - RestService host started");
                Console.WriteLine("----------------------------");
                Console.WriteLine();
                Console.WriteLine(@"  _____           _     _           ");
                Console.WriteLine(@" | ____|_  _____ | |   (_)_   _____ ");
                Console.WriteLine(@" |  _| \ \/ / _ \| |   | \ \ / / _ \");
                Console.WriteLine(@" | |___ >  < (_) | |___| |\ V /  __/");
                Console.WriteLine(@" |_____/_/\_\___/|_____|_| \_/ \___|");
                Console.WriteLine();
                Console.WriteLine("ExoLive server started!");
                Console.WriteLine("(press any key to exit)");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ExoLive server ERROR:");
                Console.WriteLine(ex.Message);
                Console.WriteLine("(press any key to exit)");
                Console.ReadLine();
            }
            finally
            {
                if (ServerEngine.Instance.GetCurrentState() == ServerState.Running)
                    ServerEngine.Instance.Stop();
            }
        }
    }
}
