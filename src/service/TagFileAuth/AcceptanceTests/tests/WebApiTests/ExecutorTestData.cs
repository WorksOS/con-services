using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Cache.MemoryCache;
using VSS.Common.Exceptions;
using VSS.Common.ServiceDiscovery;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.TagFileAuth.Abstractions.Interfaces;
using VSS.Productivity3D.TagFileAuth.Proxy;
using VSS.Serilog.Extensions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace WebApiTests
{
  public class ExecutorTestData
  {
    protected IServiceProvider serviceProvider;
    protected ILogger logger;
    protected IConfigurationStore configStore;

    protected ITagFileAuthProjectProxy tagFileAuthProjectProxy;

    //// this SNM940 exists on `VSS-TagFileAuth-Alpha` with a valid 3d sub (it's not on Dev)
    //// Dims project and customer are on alpha tfa
    //if (request.RadioSerial == "5051593854")
    //  return new GetProjectAndAssetUidsEarthWorksResult(ConstantsUtil.DIMENSIONS_PROJECT_UID, "039c1ee8-1f21-e311-9ee2-00505688274d", ConstantsUtil.DIMENSIONS_CUSTOMER_UID, true);

    protected string dimensionsRadioSerial = "5051593854";

    public ExecutorTestData()
    {
      serviceProvider = new ServiceCollection()
                        .AddLogging()
                        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.TagFileAuth.WepApiTests.log")))
                        .AddSingleton<IConfigurationStore, GenericConfiguration>()
                        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()

                        // for serviceDiscovery
                        .AddServiceDiscovery()
                        .AddTransient<IWebRequest, GracefulWebRequest>()
                        .AddMemoryCache()
                        .AddSingleton<IDataCache, InMemoryDataCache>()

                        .AddTransient<ITagFileAuthProjectProxy, TagFileAuthProjectV4Proxy>()
                        .BuildServiceProvider();

      logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ExecutorTestData>();
      configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      tagFileAuthProjectProxy = serviceProvider.GetRequiredService<ITagFileAuthProjectProxy>();
    }

  }
}
