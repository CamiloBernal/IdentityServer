using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.EntityFramework;
using ClientStore = Stores.ClientStore;
using ScopeStore = Stores.ScopeStore;

namespace AuthServer
{
    internal static class Factory
    {
        public static IdentityServerServiceFactory Configure(string nameOrConnectionString)
        {
            var efConfig = new EntityFrameworkServiceOptions
            {
                ConnectionString = nameOrConnectionString,
                SynchronousReads = false               
            };

            var factory = new IdentityServerServiceFactory();
            var scopeStore = new ScopeStore(nameOrConnectionString);
            factory.ScopeStore = new Registration<IScopeStore>(scopeStore);
            var clientStore = new ClientStore(nameOrConnectionString);
            factory.ClientStore = new Registration<IClientStore>(clientStore);
            //Enable CORS
            factory.CorsPolicyService = new Registration<ICorsPolicyService>(new DefaultCorsPolicyService { AllowAll = true });

            factory.RegisterConfigurationServices(efConfig);
            factory.RegisterOperationalServices(efConfig);

            return factory;
        }
    }
}