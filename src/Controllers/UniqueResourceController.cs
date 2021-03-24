using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class UniqueResourceController : Controller
    {
        private readonly IUniqueResource _uniqueResource;
        
        public UniqueResourceController(IUniqueResource uniqueResource)
        {
            _uniqueResource = uniqueResource;
        }

        [HttpGet("unique")]
        public async Task<IActionResult> Own(TimeSpan duration, CancellationToken cancellationToken)
        {
            await _uniqueResource.Own(duration, cancellationToken);

            return Ok("Success");
        }
    }
}