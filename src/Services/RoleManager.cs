using Core;
using Microsoft.AspNet.Identity;
using Stores;

namespace Services
{
    public class RoleManager : RoleManager<Role>
    {
        public RoleManager(RoleStore store)
            : base(store)
        {
        }
    }
}