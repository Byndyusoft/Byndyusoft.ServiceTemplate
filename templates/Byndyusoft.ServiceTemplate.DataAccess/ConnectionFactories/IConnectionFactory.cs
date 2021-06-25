namespace Byndyusoft.ServiceTemplate.DataAccess.ConnectionFactories
{
    using System.Data;
    using System.Threading.Tasks;

    public interface IConnectionFactory
    {
        Task<IDbConnection> CreateConnection();
    }
}