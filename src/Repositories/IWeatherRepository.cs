using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication1.Repositories
{
    public interface IWeatherRepository
    {
        Task<List<WeatherDto>> GetByLocation(
            string location,
            CancellationToken cancellationToken);
    }
}