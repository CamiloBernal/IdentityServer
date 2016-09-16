using IdentityServer.AspNetIdentityServices.Helpers;
using IdentityServer.Core;
using IdentityServer3.AspNetIdentity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;

namespace IdentityServer.AspNetIdentityServices
{
    public static class UserServiceFactory
    {
        public static async Task<AspNetIdentityUserService<IdentityUser, string>> Create(string nameOrConnectionString, IPopulateDbProvider populateDbProvider)
        {
            var context = new IdentityDbContext(nameOrConnectionString);
            var userStore = new UserStore<IdentityUser>(context);
            var userManager = new UserManager<IdentityUser>(userStore);
            await PopulateUsersHelper.PopulateUsers(context, userManager, populateDbProvider).ConfigureAwait(false);
            return new AspNetIdentityUserService<IdentityUser, string>(userManager);
        }
    }
}