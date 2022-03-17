namespace TestSerilog.Services
{
    public interface ITestService
    {
        void TestMethod();

        void TestException();
    }
    public class TestService : ITestService
    {
        private readonly ILogger<TestService> _logger;

        public TestService(ILogger<TestService> logger)
        {
            _logger = logger;
        }

        public void TestException()
        {
            throw new NotImplementedException("Test exception method is not implemented");
        }

        public void TestMethod()
        {
            _logger.LogInformation("This is a test method.");
        }
    }
}
