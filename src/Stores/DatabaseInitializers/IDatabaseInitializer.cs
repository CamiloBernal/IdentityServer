namespace Stores.DatabaseInitializers
{
    public interface IDatabaseInitializer
    {
        void InitializeDatabase(bool force);
    }
}