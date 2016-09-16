using IdentityServer.Core;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Threading.Tasks;

namespace IdentityServer.AspNetIdentityServices.Helpers
{
    internal static class PopulateUsersHelper
    {
        public static async Task PopulateUsers(IdentityDbContext context, UserManager<IdentityUser> userManager, IPopulateDbProvider populateDbProvider)
        {
            if (!(await context.Users.AnyAsync().ConfigureAwait(false)))
            {
                foreach (var dtoUser in populateDbProvider.Users)
                {
                    var user = new IdentityUser
                    {
                        UserName = dtoUser.UserName,
                    };
                    var result = userManager.Create(user, dtoUser.Password);
                    if (!result.Succeeded) continue;
                    var userCreated = await userManager.FindByNameAsync(user.UserName).ConfigureAwait(false);
                    var userId = userCreated.Id;
                    foreach (var claim in dtoUser.UserClaims)
                    {
                        await userManager.AddClaimAsync(userId, claim).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}