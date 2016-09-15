using System;
using System.Data.Entity;
using System.Reflection;

namespace Stores.CodeBase
{
    public class DbContextBase:DbContext
    {

        public DbContextBase(string nameOrConnectionString)
            :base(nameOrConnectionString)
        {
            //Default CTOR
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
            modelBuilder.Configurations.AddFromAssembly(Assembly.GetAssembly(typeof(DbContextBase))); //Current Assembly
            base.OnModelCreating(modelBuilder);
        }
    }
}