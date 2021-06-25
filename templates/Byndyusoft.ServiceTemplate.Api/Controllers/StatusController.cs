namespace Byndyusoft.ServiceTemplate.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    [ApiVersionNeutral]
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        [HttpGet]
        public ActionResult<bool> Index()
        {
            return true;
        }
    }
}