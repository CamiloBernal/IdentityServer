using Stores.CodeBase;
using Stores.DatabaseContexts;
using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Stores.DatabaseInitializers
{
    internal sealed class DefaultDatabaseInitializer<TContext> : IDatabaseInitializer<TContext>
        where TContext : DbContextBase
    {
        private readonly string _nameOrConnectionString;

        public DefaultDatabaseInitializer(string nameOrConnectionString)
        {
            _nameOrConnectionString = nameOrConnectionString;
        }

        private async Task SeedAsync()
        {
            //Initialize Scopes
            using (var scopeContext = new ScopeDbContext(_nameOrConnectionString))
            {
                await InitializeHelper.InitializeScopesAsync(scopeContext).ConfigureAwait(false);
            }

            //Initialize Clients
            using (var clientDbContext = new ClientDbContext(_nameOrConnectionString))
            {
                await InitializeHelper.InitializeClientsAsync(clientDbContext).ConfigureAwait(false);
            }
        }

        public async void InitializeDatabase(TContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (context.Database.Exists()) return;
            context.Database.CreateIfNotExists();
            await SeedAsync().ConfigureAwait(false);
        }
    }
}