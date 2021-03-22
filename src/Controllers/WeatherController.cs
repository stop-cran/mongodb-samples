using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Repositories;

namespace WebApplication1.Controllers
{
    public class WeatherController : Controller
    {
        private readonly IWeatherRepository weatherRepository;
        
        public WeatherController(IWeatherRepository weatherRepository)
        {
            this.weatherRepository = weatherRepository;
        }

        [HttpGet("weather")]
        public async Task<List<WeatherDto>> Get(string location, CancellationToken cancellationToken) =>
            await weatherRepository.GetByLocation(location, cancellationToken);
    }
}