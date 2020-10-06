namespace Byndyusoft.ServiceTemplate.Api.HealthChecks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain.Services.Interfaces;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    /// <summary>
    ///     Пример хелсчека, работающего с БД
    /// </summary>
    public class ExampleHealthCheck : IHealthCheck
    {
        private readonly IDatabaseService _databaseService;

        public ExampleHealthCheck(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                await _databaseService.SampleSelect();

                return HealthCheckResult.Healthy("ExampleHealthCheck could connect to database");
            }
            catch (Exception)
            {
                return HealthCheckResult.Unhealthy("ExampleHealthCheck could not connect to database");
            }
        }
    }
}