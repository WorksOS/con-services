using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Serilog.Extensions;

namespace VSS.MasterData.Models.UnitTests
{
  public class BaseTest
  {
    protected IServiceProvider ServiceProvider;

    public virtual void InitTest()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.MasterData.Models.UnitTests.log"));
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging()
                       .AddSingleton(loggerFactory)
                       .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
                       .AddTransient<IErrorCodesProvider, FilterValidationErrorCodesProvider>();

      ServiceProvider = serviceCollection.BuildServiceProvider();
    }
  }
}
