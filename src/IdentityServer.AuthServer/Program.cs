using Microsoft.Owin.Hosting;
using System;

namespace IdentityServer.AuthServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string url = "https://localhost:44333/core";

            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine($"\n\nServer listening at {url}. Press enter to stop");
                Console.ReadLine();
            }
        }
    }
}