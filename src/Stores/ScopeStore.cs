using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Stores.DatabaseContexts;

namespace Stores
{
    public class ScopeStore : IScopeStore
    {
        private readonly ScopeDbContext _dbContext;
        public ScopeStore(string nameOrConnectionString)
        {
            _dbContext = new ScopeDbContext(nameOrConnectionString);
        }

        public async Task<IEnumerable<Scope>> FindScopesAsync(IEnumerable<string> scopeNames)
        {
            if (scopeNames == null) throw new ArgumentNullException(nameof(scopeNames));

            var scopes = from s in _dbContext.Scopes
                         where scopeNames.ToList().Contains(s.Name)
                         select s;
            return await scopes.ToListAsync().ConfigureAwait(false);
        }

        public async Task<IEnumerable<Scope>> GetScopesAsync(bool publicOnly = true)
        {
            var scopes = _dbContext.Scopes;
            return publicOnly
                ? await scopes.Where(s => s.ShowInDiscoveryDocument).ToListAsync().ConfigureAwait(false)
                : await scopes.ToListAsync().ConfigureAwait(false);
        }
    }
}