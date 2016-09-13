using System.Data.Entity;

namespace Stores.CodeBase
{
    public class DbContextBase:DbContext
    {

        public DbContextBase(string nameOrConnectionString)
            :base(nameOrConnectionString)
        {
            //Default CTOR
        }

    }
}