using System;
using IdentityServer3.AspNetIdentity;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Threading.Tasks;

namespace IdentityServer.AspNetIdentityServices
{
    public static class IdentityServerServiceFactoryIdentityExtensions
    {
        public static void UseAspNetIdentity(this IdentityServerServiceFactory factory, string connString)
        {
            factory.Register(new Registration<IdentityDbContext>());
            factory.Register(new Registration<UserStore<IdentityUser>>());
            factory.Register(new Registration<UserManager<IdentityUser, string>>(x => new UserManager<IdentityUser>(x.Resolve<UserStore<IdentityUser>>())));
            //factory.UserService = new Registration<IUserService, AspNetIdentityUserService<IdentityUser, string>>();
            try
            {
                factory.UserService = new Registration<IUserService>(UserServiceFactory.Create(connString).Result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }
    }

    public static class UserServiceFactory
    {
        public static async Task<AspNetIdentityUserService<IdentityUser, string>> Create(string nameOrConnectionString)
        {
            var context = new IdentityDbContext(nameOrConnectionString);
            var userStore = new UserStore<IdentityUser>(context);
            var userManager = new UserManager<IdentityUser>(userStore);

            if (await context.Users.AnyAsync().ConfigureAwait(false))
                return new AspNetIdentityUserService<IdentityUser, string>(userManager);
            //var myinfo = new MyUserInfo() { FirstName = "Pranav", LastName = "Rastogi" };
            string name = "Admin";
            string password = "123456";
            string test = "admin";

            //Create User=Admin with password=123456
            var user = new IdentityUser
            {
                UserName = name,
                //Id = System.Guid.NewGuid().ToString()
            };

            //user.Claims.Add(new IdentityUserClaim
            //{
            //    ClaimType = IdentityServer3.Core.Constants.ClaimTypes.GivenName,
            //    ClaimValue = "Admin"
            //});
            //user.Claims.Add(new IdentityUserClaim
            //{
            //    ClaimType = IdentityServer3.Core.Constants.ClaimTypes.GivenName,
            //    ClaimValue = "Scott"
            //});
            //user.Claims.Add(new IdentityUserClaim
            //{
            //    ClaimType = IdentityServer3.Core.Constants.ClaimTypes.FamilyName,
            //    ClaimValue = "Brady"
            //});
            //user.Claims.Add(new IdentityUserClaim
            //{
            //    ClaimType = IdentityServer3.Core.Constants.ClaimTypes.Email,
            //    ClaimValue = "info@scottbrady91.com"
            //});
            //user.Claims.Add(new IdentityUserClaim
            //{
            //    ClaimType = IdentityServer3.Core.Constants.ClaimTypes.Role,
            //    ClaimValue = "Badmin"
            //});

           var result =  userManager.Create(user, password);

            if (result.Succeeded)
            {
                var userCreated = await userManager.FindByNameAsync(user.UserName).ConfigureAwait(false);
                Console.WriteLine(userCreated.Id);
            }

            return new AspNetIdentityUserService<IdentityUser, string>(userManager);
        }
    }
}