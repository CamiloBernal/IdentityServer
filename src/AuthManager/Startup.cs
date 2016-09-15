using AuthManager.Extensions;
using IdentityManager.Configuration;
using Owin;
using Stores.DatabaseInitializers;

namespace AuthManager
{
    internal class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["LocalConnectionString"].ConnectionString;
            var databaseInitializer = new DefaultDatabaseInitializerFactory(connectionString);
            databaseInitializer.InitializeDatabase(true);
            var factory = new IdentityManagerServiceFactory();
            factory.ConfigureSimpleIdentityManagerService(connectionString);
            app.UseIdentityManager(new IdentityManagerOptions()
            {
                Factory = factory
            });
        }
    }
}