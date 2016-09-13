using Stores.CodeBase;
using System;
using System.Data.Entity;

namespace Stores.DatabaseInitializers
{
    public class DefaultDatabaseInitializerFactory : IDatabaseInitializer
    {
        private readonly string _nameOrConnectionString;

        public DefaultDatabaseInitializerFactory(string nameOrConnectionString)
        {
            _nameOrConnectionString = nameOrConnectionString;
        }

        public void InitializeDatabase(bool force)
        {
            Database.SetInitializer<DbContextBase>(new DefaultDatabaseInitializer<DbContextBase>(_nameOrConnectionString));
            using (var context = (DbContextBase)Activator.CreateInstance(typeof(DbContextBase), _nameOrConnectionString))
            {
                context.Database.Initialize(force);
            }
        }
    }
}