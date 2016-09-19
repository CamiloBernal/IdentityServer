using IdentityServer.AspNetIdentityServices;
using IdentityServer.DefaultPopulateDbProvider;
using IdentityServer.EFServices;
using IdentityServer3.Core.Configuration;
using Owin;
using System.Configuration;
using IdentityServer.AuthServer.Config;

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
            factory.UseAspNetIdentity(ConnectionString, populator);
            var options = new IdentityServerOptions
            {
                SiteName = "IdentityServer3 - (EntityFramework)",
                SigningCertificate = Certificate.Get(),
                Factory = factory,

                EventsOptions = new EventsOptions
                {
                    RaiseSuccessEvents = true,
                    RaiseErrorEvents = true,
                    RaiseFailureEvents = true,
                    RaiseInformationEvents = true
                },
                LoggingOptions = new LoggingOptions
                {
                    EnableHttpLogging = true,
                    EnableKatanaLogging = true,
                    EnableWebApiDiagnostics = true,
                    WebApiDiagnosticsIsVerbose = true
                }
            };

            appBuilder.UseIdentityServer(options);
        }
    }
}