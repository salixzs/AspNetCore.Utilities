using Dto;

namespace BusinessLogic
{
    public class WeatherLogic : IWeatherLogic
    {
        private static readonly Random Random = new();
        private static readonly string[] Summaries = new[]
{
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public Task<WeatherForecast> GetCurrentPrediction() =>
            Task.FromResult(new WeatherForecast
            {
                Date = DateTime.Now,
                TemperatureC = Random.Next(-20, 55),
                Summary = Summaries[Random.Next(Summaries.Length)]
            });


        public Task<IEnumerable<WeatherForecast>> GetNextFivePredictions() =>
            Task.FromResult(Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = Random.Next(-20, 55),
                    Summary = Summaries[Random.Next(Summaries.Length)]
                }));
    }
}
