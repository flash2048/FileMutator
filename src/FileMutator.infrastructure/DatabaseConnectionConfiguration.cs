
using Microsoft.Data.SqlClient;

namespace FileMutator.infrastructure
{
    public class DatabaseConnectionConfiguration
    {
        private string? ConnectionWithoutCredentialsAndInitialCatalog { get; set; }

        public string? UserName { get; set; }
        public string? InitialCatalog { get; set; }

        public DatabaseConnectionConfiguration(string connectionString)
        {
            ConnectionWithoutCredentialsAndInitialCatalog = connectionString;
        }

        public SqlConnectionStringBuilder GetConnectionString()
        {
            return new SqlConnectionStringBuilder(ConnectionWithoutCredentialsAndInitialCatalog);

        }
    }
}
