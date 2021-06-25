namespace Byndyusoft.ServiceTemplate.IntegrationTests
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Api.Client.Clients;
    using Api.Client.Settings;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using OpenTracing.Mock;
    using Xunit;

    public class TemplateIntegrationTest
    {
        private readonly TemplateClient _templateClient;

        public TemplateIntegrationTest()
        {
            _templateClient = new TemplateClient(new HttpClient(),
                                                 Options.Create(new TemplateApiSettings {Url = "http://localhost:50001"}),
                                                 new MockTracer());
        }

        [Fact]
        public async Task IntegrationTest()
        {
            var templateId = 4;
            var templateDto = await _templateClient.GetTemplate(templateId).ConfigureAwait(false);

            templateDto.Should().NotBeNull();
            templateDto.Id.Should().Be(templateId);
        }
    }
}