using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace WebApplication1.Repositories
{
    public class MongoDbWeatherRepository : IWeatherRepository
    {
        private readonly IMongoCollection<WeatherDto> _weatherDtoCollection;

        public MongoDbWeatherRepository(IMongoCollection<WeatherDto> weatherDtoCollection)
        {
            _weatherDtoCollection = weatherDtoCollection;
        }


        public async Task<List<WeatherDto>> GetByLocation(
            string location,
            CancellationToken cancellationToken)
        {
            return await _weatherDtoCollection.Find(l => l.Location == location,
                    new FindOptions
                    {
                        Collation = new Collation("en_US")
                    })
                .SortBy(l => l.ForecastDate)
                .Limit(10)
                .ToListAsync(cancellationToken);
        }
    }
}