using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using TestSerilog.Services;

namespace TestSerilog.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly ITestService _testService;

        public TestController(ILogger<TestController> logger,
            ITestService testService)
        {
            _logger = logger;
            _testService = testService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/LogInfo")]
        public IActionResult LogInfo()
        {
            var correlationId = Guid.NewGuid();

            // Property we push into Serilog's LogContext will be added to every log message withing the scope of this properties 
            // 'using' scope. Since we put this using in the controller root, CorrelationId property will be appended to every 
            // log message from this point on. This properties can be queried in database if we use MSSql Sink
            using var _correlationIdProperty = LogContext.PushProperty("CorrelationId", correlationId);

            try
            {
                _logger.LogInformation("This is LogInfo method"); // This log will contain CorrelationId property

                // This is another log property we will push. Every log message within it's scope will contain that property
                using (var _someProperty = LogContext.PushProperty("SomeProperty", "This is some property"))
                {
                    var randomGuid = Guid.NewGuid();
                    // When we want to put some data in our log message, this is the correct way to do it. 
                    // We put '{PropertyName}' into our log message where that data should be and then we
                    // pass the data as second parameter of the log method. We can add as many properties like this
                    // as we want. 
                    // Benefit of this is that every property we add like this will be pushed into the Properties
                    // column in database. 
                    _logger.LogInformation("I am doing something with {RandomGuid}", randomGuid); // This log will contain CorrelationId, SomeProperty and RandomGuid properties
                    _logger.LogInformation("I have done something with that"); // This log will contain CorrelationId and SomeProperty properties
                }

                _logger.LogInformation("Now I will call TestService"); // This log will contain CorrelationId property

                // This method will write Information log. It will also contain CorrelationId property
                _testService.TestMethod();

                _logger.LogInformation("Now I will call TestException"); // This log will contain CorrelationId property

                // This method will throw exception
                _testService.TestException();
            }
            catch (Exception exception)
            {
                // If we get exception, we can use log method which has the first parameter as Exception, and second parameter as log message.
                // We can also log additional data as '{PropertyName}' in log message
                _logger.LogError(exception, "Something wrong has happened"); // This log will contain CorrelationId property
                return StatusCode(500, correlationId);
            }

            return Ok(correlationId);
        }
    }
}