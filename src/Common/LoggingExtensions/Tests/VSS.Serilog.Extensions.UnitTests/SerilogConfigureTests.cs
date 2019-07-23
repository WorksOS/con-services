using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;

namespace VSS.Serilog.Extensions.UnitTests
{
  public class SerilogConfigureTests
  {
    [Fact]
    public void Should_not_throw_if_appsettingsjson_not_found()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Serilog.Extensions.UnitTests.log"));

      new ServiceCollection().AddLogging()
                             .AddSingleton(loggerFactory)
                             .BuildServiceProvider();
    }
  }
}
