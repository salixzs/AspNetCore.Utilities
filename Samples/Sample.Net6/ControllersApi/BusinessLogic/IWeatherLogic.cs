using Dto;

namespace BusinessLogic
{
    public interface IWeatherLogic
    {
        Task<IEnumerable<WeatherForecast>> GetNextFivePredictions();

        Task<WeatherForecast> GetCurrentPrediction();
    }
}
