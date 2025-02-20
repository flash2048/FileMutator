using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Serilog;

namespace FileMutator.DbMigrate.Helpers
{
    public static class MigrationHelper
    {
        public static async Task MigrateMultipleContextsAsync(params DbContext[] contexts)
        {
            var pendingMigrations = new List<Migration>();

            foreach (var context in contexts)
            {
                var migrator = context.GetService<IMigrator>();
                var pendingNames = (await context.Database.GetPendingMigrationsAsync()).ToArray();
                var migrations = context.Database.GetMigrations()
                    .Select(migrationName => new Migration(context, migrationName, migrator, pendingNames.Contains(migrationName, StringComparer.Ordinal)))
                    .ToArray();

                VerifyFutureAppliedMigrations(migrations);

                pendingMigrations.AddRange(migrations.Where(e => e.Pending));
            }

            if (pendingMigrations.Count == 0)
            {
                Log.Logger.Information("No new migrations to apply");
                return;
            }

            foreach (var migration in pendingMigrations.OrderBy(m => m.CreationTime))
            {
                Log.Logger.Information("Applying migration - db: {database}, context: {context}, migration: {migrationsCommon}",
                    migration.Context.Database.GetDbConnection().Database,
                    migration.Context.GetType().Name,
                    migration.Name);

                await migration.MigrateAsync();
            }

            Log.Logger.Information("All migrations were applied successfully");
        }

        private static void VerifyFutureAppliedMigrations(IEnumerable<Migration> migrations)
        {
            // Verify that there is no applied future migrations: applied, applied, pending, applied
            var pendingReached = false;
            foreach (var migration in migrations.OrderBy(e => e.CreationTime))
            {
                if (migration.Pending)
                {
                    pendingReached = true;
                }
                else if (pendingReached)
                {
                    // Future migration will be reverted and reapplied in new order with pending one. This may corrupt existing data.
                    throw new Exception($"Applied future migration detected, this is not allowed - {migration}");
                }
            }
        }

        private class Migration
        {
            private IMigrator Migrator { get; }
            public DbContext Context { get; }
            public string Name { get; }
            public DateTime CreationTime { get; }
            public bool Pending { get; }

            public Migration(DbContext context, string name, IMigrator migrator, bool pending)
            {
                Context = context;
                Name = name;

                var timeStr = name.Substring(0, name.IndexOf('_', StringComparison.Ordinal));
                CreationTime = DateTime.ParseExact(timeStr, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                Migrator = migrator;
                Pending = pending;
            }

            public Task MigrateAsync()
            {
                return Migrator.MigrateAsync(Name);
            }

            public override string ToString()
            {
                var context = Context.GetType().Name;
                var status = Pending ? "Pending" : "Applied";
                return $"{context}.{Name}: {status}";
            }
        }
    }
}
