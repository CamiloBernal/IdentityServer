using Core;
using Microsoft.AspNet.Identity;
using Stores;

namespace Services
{
    public class UserManager : UserManager<User, string>
    {
        public UserManager(UserStore store)
            : base(store)
        {
            this.ClaimsIdentityFactory = new ClaimsFactory();
        }
    }
}