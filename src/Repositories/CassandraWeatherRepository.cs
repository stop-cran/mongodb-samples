using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Mapping;
using WebApplication1.Awaitable;

namespace WebApplication1.Repositories
{
    public class CassandraWeatherRepository : IWeatherRepository
    {
        private readonly ITaskAwaitable<ISession> _sessionAwaitable;

        public CassandraWeatherRepository(ITaskAwaitable<ISession> sessionAwaitable)
        {
            _sessionAwaitable = sessionAwaitable;
        }

        public async Task<List<WeatherDto>> GetByLocation(
            string location,
            CancellationToken cancellationToken)
        {
            var session = await _sessionAwaitable;
            var mapper = new Mapper(session);

            var result =
                await mapper.FetchAsync<WeatherDtoC>("select * from app.weather where location = ? limit 10 allow filtering", location);

            return result.Select(r => new WeatherDto
            {
                Location = r.Location,
                ForecastDate = r.ForecastDate,
                TemperatureC = r.TemperatureC
            }).ToList();
        }
    }
}