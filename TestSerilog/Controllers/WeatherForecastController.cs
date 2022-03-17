using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using TestSerilog.Services;

namespace TestSerilog.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ITestService _testService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
            ITestService testService)
        {
            _logger = logger;
            _testService = testService;
        }

        [HttpGet]
        [Route("GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            _logger.LogInformation("Fetching weather forcast");
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet]
        [Route("/LogInfo")]
        public IActionResult LogInfo()
        {
            var correlationId = Guid.NewGuid();

            using var _correlationIdProperty = LogContext.PushProperty("CorrelationId", correlationId);

            try
            {
                _logger.LogInformation("This is LogInfo method");

                using (var _someProperty = LogContext.PushProperty("SomeProperty", "This is some property"))
                {
                    var randomGuid = Guid.NewGuid();
                    _logger.LogInformation("I am doing something with {RandomGuid}", randomGuid);
                    _logger.LogInformation("I have done something with that");
                }

                _logger.LogInformation("Now I will call TestService");

                _testService.TestMethod();

                _logger.LogInformation("Now I will call TestException");

                _testService.TestException();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Something wrong has happened");
                return StatusCode(500, correlationId);
            }

            return Ok(correlationId);
        }
    }
}