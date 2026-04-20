using Microsoft.Extensions.Logging;

namespace SamplePackage;

public class SampleService(ILogger<SampleService> logger) : ISampleService
{
    public void DoSomething()
    {
        logger.LogInformation("SampleService is doing something!");
    }
}