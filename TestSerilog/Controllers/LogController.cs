using Microsoft.AspNetCore.Mvc;
using TestSerilog.Dto;
using TestSerilog.Services;

namespace TestSerilog.Controllers
{
    [Route("[Controller]")]
    public class LogController : ControllerBase
    {
        private readonly ILogQueryService _logQueryService;

        public LogController(ILogQueryService logQueryService)
        {
            _logQueryService = logQueryService;
        }

        /// <summary>
        /// Query logs by log properties
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult QueryLogs(LogQueryDto model)
        {
            var logs = _logQueryService.Query(model);
            return Ok(logs);
        }
    }
}
