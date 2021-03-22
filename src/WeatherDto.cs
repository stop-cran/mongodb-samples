using System;
using MongoDB.Bson;

namespace WebApplication1
{
    public class WeatherDto
    {
        public ObjectId Id { get; set; }
        public DateTime ForecastDate { get; set; }
        public string Location { get; set; }
        public double TemperatureC { get; set; }
    }
}