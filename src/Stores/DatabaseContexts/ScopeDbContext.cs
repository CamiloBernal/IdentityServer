using IdentityServer3.Core.Models;
using System.Data.Entity;
using Stores.CodeBase;

namespace Stores.DatabaseContexts
{
    internal class ScopeDbContext : DbContextBase
    {
        public ScopeDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            //Default Ctor
        }

        public DbSet<Scope> Scopes { get; set; }
    }
}