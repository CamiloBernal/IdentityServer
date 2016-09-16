using IdentityServer3.Core.Models;
using System.Collections.Generic;

namespace IdentityServer.Core
{
    public interface IPopulateDbProvider
    {
        IEnumerable<Client> Clients { get; }
        IEnumerable<Scope> Scopes { get; }
    }
}