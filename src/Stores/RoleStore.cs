using Core;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Stores
{
    public class RoleStore : RoleStore<Role>
    {
        public RoleStore(Context ctx)
            : base(ctx)
        {
        }
    }
}