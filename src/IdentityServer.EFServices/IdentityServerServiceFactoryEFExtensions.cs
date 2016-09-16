using IdentityServer.Core;
using IdentityServer.EFServices.Helpers;
using IdentityServer3.Core.Configuration;
using IdentityServer3.EntityFramework;

namespace IdentityServer.EFServices
{
    public static class IdentityServerServiceFactoryEfExtensions
    {
        public static void UseEntityFramework(this IdentityServerServiceFactory factory, string connectionString, IPopulateDbProvider populateDbProvider)
        {
            var efConfig = new EntityFrameworkServiceOptions
            {
                ConnectionString = connectionString,
                //SynchronousReads = true
            };
            // these two calls just pre-populate the test DB from the in-memory config
            ConfigurationHelper.ConfigureClients(populateDbProvider.Clients, efConfig);
            ConfigurationHelper.ConfigureScopes(populateDbProvider.Scopes, efConfig);
            factory.RegisterConfigurationServices(efConfig);
            factory.RegisterOperationalServices(efConfig);
            factory.ConfigureClientStoreCache();
            factory.ConfigureScopeStoreCache();
        }
    }
}