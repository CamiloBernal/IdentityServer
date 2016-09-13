using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Stores.DatabaseContexts;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Stores
{
    public class ClientStore : IClientStore
    {
        private readonly ClientDbContext _dbContext;

        public ClientStore(string nameOrConnectionString)
        {
            _dbContext = new ClientDbContext(nameOrConnectionString);
        }

        public async Task<Client> FindClientByIdAsync(string clientId) => await _dbContext.Clients.FirstOrDefaultAsync(c => c.ClientId == clientId).ConfigureAwait(false);
    }
}