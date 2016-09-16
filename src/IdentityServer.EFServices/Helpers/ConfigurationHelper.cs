using IdentityServer3.Core.Models;
using IdentityServer3.EntityFramework;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer.EFServices.Helpers
{
    public class ConfigurationHelper
    {
        public static void ConfigureClients(IEnumerable<Client> clients, EntityFrameworkServiceOptions options)
        {
            using (var db = new ClientConfigurationDbContext(options.ConnectionString, options.Schema))
            {
                if (db.Clients.Any()) return;
                foreach (var c in clients)
                {
                    var e = c.ToEntity();
                    db.Clients.Add(e);
                }
                db.SaveChanges();
            }
        }

        public static void ConfigureScopes(IEnumerable<Scope> scopes, EntityFrameworkServiceOptions options)
        {
            using (var db = new ScopeConfigurationDbContext(options.ConnectionString, options.Schema))
            {
                if (db.Scopes.Any()) return;
                foreach (var s in scopes)
                {
                    var e = s.ToEntity();
                    db.Scopes.Add(e);
                }
                db.SaveChanges();
            }
        }
    }
}