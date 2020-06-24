using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Serilog.Extensions;

namespace CoreX.Wrapper.UnitTests
{
  public class UnitTestBaseFixture : IDisposable
  {
    public IServiceProvider serviceProvider;

    public UnitTestBaseFixture()
    {
      serviceProvider = new ServiceCollection()
                        .AddLogging()
                        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("CoreX.Wrapper.UnitTests.log")))
                        .AddSingleton<ICoordinateServiceUtility, CoordinateServiceUtility>()
                        .AddSingleton<IConvertCoordinates, ConvertCoordinates>()
                        .BuildServiceProvider();
    }

    public void Dispose()
    { }
  }
}
