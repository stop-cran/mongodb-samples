using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Repositories;

namespace WebApplication1.Controllers
{
    public class WeatherController : Controller
    {
        private readonly IWeatherRepository _weatherRepository;

        public WeatherController(IWeatherRepository weatherRepository)
        {
            _weatherRepository = weatherRepository;
        }

        [HttpGet("weather")]
        public async Task<List<WeatherDto>> Get(string location, CancellationToken cancellationToken)
        {
            return await _weatherRepository.GetByLocation(location, cancellationToken);
        }
    }
}