using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer.Core.Dtos
{
    public class IdentityUserDto : IUser<string>
    {
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public string Id { get; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public IEnumerable<Claim> UserClaims { get; set; }
    }
}