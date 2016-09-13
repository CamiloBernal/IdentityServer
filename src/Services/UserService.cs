using Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class UserService : AspNetIdentityUserService<User, string>
    {
        public UserService(UserManager userMgr)
            : base(userMgr)
        {
        }

        protected override async Task<IEnumerable<System.Security.Claims.Claim>> GetClaimsFromAccount(User user)
        {
            var claims = (await base.GetClaimsFromAccount(user).ConfigureAwait(false)).ToList();
            if (!string.IsNullOrWhiteSpace(user.FirstName))
            {
                claims.Add(new System.Security.Claims.Claim("given_name", user.FirstName));
            }
            if (!string.IsNullOrWhiteSpace(user.LastName))
            {
                claims.Add(new System.Security.Claims.Claim("family_name", user.LastName));
            }

            return claims;
        }
    }
}