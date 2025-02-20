using FileMutator.infrastructure;
using FileMutator.infrastructure.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;
using Serilog.Extensions.Logging;

namespace FileMutator.DbMigrate
{
    class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FileMutatorDbContext>
    {
        private readonly DatabaseConnectionConfiguration _configuration;

        public DesignTimeDbContextFactory()
        {
            var connectionString = Environment.GetEnvironmentVariable(FileMutatorConstants.DbConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException($"The '{FileMutatorConstants.DbConnectionStringName}' don't contains value!");
            _configuration = new DatabaseConnectionConfiguration(connectionString);
        }

        public DesignTimeDbContextFactory(DatabaseConnectionConfiguration configuration)
        {
            _configuration = configuration;
        }

        public FileMutatorDbContext CreateDbContext(string[]? args = null)
        {
            Log.Logger.Information("Connecting to database - db: {database}, user: {user}", _configuration.InitialCatalog, _configuration.UserName);
            var optionsBuilder = new DbContextOptionsBuilder<FileMutatorDbContext>()
                .UseSqlServer(_configuration.GetConnectionString().ConnectionString,
                    x => x.MigrationsAssembly("FileMutator.DbMigrate")
                        .MigrationsHistoryTable("__MigrationsHistory")
                        .EnableRetryOnFailure())
                .ConfigureWarnings(w => w.Log(RelationalEventId.MigrationReverting)
                    .Ignore(RelationalEventId.CommandExecuted)
                    .Ignore(RelationalEventId.CommandExecuting))
                .UseLoggerFactory(new SerilogLoggerFactory())
                .EnableDetailedErrors();

            return new FileMutatorDbContext(optionsBuilder.Options);
        }
    }
}
