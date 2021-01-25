namespace Byndyusoft.ServiceTemplate.DataAccess
{
    using System.Linq;
    using System.Threading.Tasks;
    using Dapper;
    using Domain.Services.Interfaces;
    using IConnectionFactory = ConnectionFactories.IConnectionFactory;

    public class DatabaseService : IDatabaseService
    {
        private readonly IConnectionFactory _connectionFactory;

        public DatabaseService(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<int[]> SampleSelect()
        {
            using var connection = await _connectionFactory.CreateConnection().ConfigureAwait(false);

            var ints = await connection.QueryAsync<int>(@"SELECT 1").ConfigureAwait(false);

            return ints.ToArray();
        }
    }
}