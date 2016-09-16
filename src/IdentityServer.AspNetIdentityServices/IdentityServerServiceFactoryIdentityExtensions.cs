using IdentityServer.Core;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;

namespace IdentityServer.AspNetIdentityServices
{
    public static class IdentityServerServiceFactoryIdentityExtensions
    {
        public static void UseAspNetIdentity(this IdentityServerServiceFactory factory, string connString, IPopulateDbProvider populateDbProvider)
        {
            factory.Register(new Registration<IdentityDbContext>());
            factory.Register(new Registration<UserStore<IdentityUser>>());
            factory.Register(new Registration<UserManager<IdentityUser, string>>(x => new UserManager<IdentityUser>(x.Resolve<UserStore<IdentityUser>>())));
            //factory.UserService = new Registration<IUserService, AspNetIdentityUserService<IdentityUser, string>>();
            try
            {
                factory.UserService = new Registration<IUserService>(UserServiceFactory.Create(connString, populateDbProvider).Result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}