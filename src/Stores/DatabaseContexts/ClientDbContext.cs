using IdentityServer3.Core.Models;
using System.Data.Entity;
using Stores.CodeBase;

namespace Stores.DatabaseContexts
{
    internal class ClientDbContext : DbContextBase
    {
        public ClientDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            //Default CTOR
        }

        public DbSet<Client> Clients { get; set; }
    }
}