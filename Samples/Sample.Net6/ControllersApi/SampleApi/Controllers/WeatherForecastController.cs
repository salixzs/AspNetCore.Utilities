using BusinessLogic;
using Dto;
using Microsoft.AspNetCore.Mvc;

namespace SampleApi.Controllers
{
    [ApiController]
    [Route("weather")]
    public class WeatherForecastController : ControllerBase
    {
        private IWeatherLogic _logic;

        public WeatherForecastController(IWeatherLogic logic) => _logic = logic;

        [HttpGet]
        public async Task<WeatherForecast> GetCurrent() => await _logic.GetCurrentPrediction();

        [HttpGet("next5")]
        public async Task<IEnumerable<WeatherForecast>> GetNext5() => await _logic.GetNextFivePredictions();
    }
}
