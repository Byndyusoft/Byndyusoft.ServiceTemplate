namespace Byndyusoft.ServiceTemplate.DataAccess.ConnectionFactories
{
    using System.Data;
    using System.Threading.Tasks;
    using Domain.Settings;
    using Microsoft.Extensions.Options;
    using Npgsql;

    public class NpgConnectionFactory : IConnectionFactory
    {
        private readonly DatabaseConnectionSettings _settings;

        public NpgConnectionFactory(IOptions<DatabaseConnectionSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<IDbConnection> CreateConnection()
        {
            var connection = new NpgsqlConnection(_settings.ConnectionString);

            await connection.OpenAsync().ConfigureAwait(false);

            return connection;
        }
    }
}