using Stores.DatabaseContexts;
using System.Threading.Tasks;

namespace Stores.DatabaseInitializers
{
    internal static class InitializeHelper
    {
        public static async Task InitializeScopesAsync(ScopeDbContext dbContext)
        {
            var scopes = SeedFactory.GetScopes();
            dbContext.Scopes.AddRange(scopes);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public static async Task InitializeClientsAsync(ClientDbContext dbContext)
        {
            var clients = SeedFactory.GetClients();
            dbContext.Clients.AddRange(clients);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}