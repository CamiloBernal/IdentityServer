using Microsoft.Owin.Hosting;
using System;
using Serilog;

namespace IdentityServer.AuthServer
{
    internal class Program
    {
        private static void Main()
        {

            Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Debug()
           //.WriteTo.Trace()
           //.WriteTo.File(@"c:\logs\ef-sample.txt")
           .WriteTo.LiterateConsole()
           .CreateLogger();

            const string url = "https://localhost:44333/core";

            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine($"\n\nServer listening at {url}. Press enter to stop");
                Console.ReadLine();
            }
        }
    }
}