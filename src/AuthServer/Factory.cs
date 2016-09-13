using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using Stores;

namespace AuthServer
{
    internal static class Factory
    {
        public static IdentityServerServiceFactory Configure(string nameOrConnectionString)
        {
            var factory = new IdentityServerServiceFactory();
            var scopeStore = new ScopeStore(nameOrConnectionString);
            factory.ScopeStore = new Registration<IScopeStore>(scopeStore);
            var clientStore = new ClientStore(nameOrConnectionString);
            factory.ClientStore = new Registration<IClientStore>(clientStore);
            //Enable CORS
            factory.CorsPolicyService = new Registration<ICorsPolicyService>(new DefaultCorsPolicyService { AllowAll = true });
            return factory;
        }
    }
}