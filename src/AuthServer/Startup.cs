using IdentityServer3.Core.Configuration;
using Owin;
using Services;
using Stores.DatabaseInitializers;

namespace AuthServer
{
    internal class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["LocalConnectionString"].ConnectionString;

            var databaseInitializer = new DefaultDatabaseInitializerFactory(connectionString);
            databaseInitializer.InitializeDatabase(true);
            var factory = Factory.Configure(connectionString);

            factory.ConfigureUserService(connectionString);
            //idSvrFactory.ConfigureCustomUserService("AspId_CustomPK");

            

            var options = new IdentityServerOptions
            {
                SiteName = "Demo Identity server",                
                Factory = factory,                
            };

            appBuilder.UseIdentityServer(options);
        }
    }
}