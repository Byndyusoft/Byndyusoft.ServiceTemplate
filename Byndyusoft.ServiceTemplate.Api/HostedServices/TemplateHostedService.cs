namespace Byndyusoft.ServiceTemplate.Api.HostedServices
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    //TODO переименовать в настоящее название вашей службы
    public class TemplateHostedService : IHostedService
    {
        private readonly ILogger<TemplateHostedService> _logger;

        public TemplateHostedService(ILogger<TemplateHostedService> logger)
        {
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Background service started");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}