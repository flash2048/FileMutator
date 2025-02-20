using FileMutator.DbMigrate.Helpers;
using FileMutator.infrastructure;

namespace FileMutator.DbMigrate
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable(FileMutatorConstants.DbConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException($"The '{FileMutatorConstants.DbConnectionStringName}' don't contains value!");

            var envDatabaseConfiguration = new DatabaseConnectionConfiguration(connectionString);
            await MigrateDbAsync(envDatabaseConfiguration);
        }

        private static async Task MigrateDbAsync(DatabaseConnectionConfiguration databaseConnectionConfiguration)
        {
            await using var fileMutatorDbContext = new DesignTimeDbContextFactory(databaseConnectionConfiguration).CreateDbContext();
            await MigrationHelper.MigrateMultipleContextsAsync(fileMutatorDbContext);
        }
    }
}
