namespace Byndyusoft.ServiceTemplate.Api.HealthChecks
{
    using Domain.Settings;
    using global::HealthChecks.NpgSql;
    using Microsoft.Extensions.Options;

    /// <inheritdoc />
    public class CustomNpgSqlHealthCheck : NpgSqlHealthCheck
    {
        /// <inheritdoc />
        public CustomNpgSqlHealthCheck(IOptions<DatabaseConnectionSettings> options) : base(options.Value.ConnectionString, "SELECT 1")
        {
            
        }
    }
}