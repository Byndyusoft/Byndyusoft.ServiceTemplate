namespace Byndyusoft.ServiceTemplate.DataAccess.ConnectionFactories
{
    using System.Data;
    using System.Threading.Tasks;
    using Domain.Settings;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Options;

    public class MsSqlConnectionFactory : IConnectionFactory
    {
        private readonly DatabaseConnectionSettings _settings;

        public MsSqlConnectionFactory(IOptions<DatabaseConnectionSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<IDbConnection> CreateConnection()
        {
            var connection = new SqlConnection(_settings.ConnectionString);

            await connection.OpenAsync().ConfigureAwait(false);

            return connection;
        }
    }
}