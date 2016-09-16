using IdentityServer.AspNetIdentityServices;
using IdentityServer.DefaultPopulateDbProvider;
using IdentityServer.EFServices;
using IdentityServer3.Core.Configuration;
using Owin;
using System.Configuration;

namespace IdentityServer.AuthServer
{
    internal class Startup
    {
        private static string ConnectionString
            => ConfigurationManager.ConnectionStrings["LocalConnectionString"].ConnectionString;

        public void Configuration(IAppBuilder appBuilder)
        {
            var factory = new IdentityServerServiceFactory();

            var populator = new Populator();

            factory.UseEntityFramework(ConnectionString, populator);
            factory.UseAspNetIdentity(ConnectionString);

            var options = new IdentityServerOptions
            {
                SiteName = "IdentityServer3 - (EntityFramework)",
                //SigningCertificate = Certificate.Get(),
                Factory = factory
            };

            appBuilder.UseIdentityServer(options);
        }
    }
}