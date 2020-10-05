namespace Byndyusoft.ServiceTemplate.Api.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using OpenTracing;
    using Shared.Dtos;

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TemplatesController : ControllerBase
    {
        private readonly ILogger<TemplatesController> _logger;
        private readonly ITracer _tracer;

        public TemplatesController(ILogger<TemplatesController> logger,
                                   ITracer tracer)
        {
            _logger = logger;
            _tracer = tracer;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TemplateDto>> Get(int id)
        {
            _tracer.ActiveSpan.SetTag(nameof(id), id);

            _logger.LogInformation("Get some id {Id}", id);

            var result = await Task.FromResult(new TemplateDto {Id = id}).ConfigureAwait(false);

            return result;
        }
    }
}