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
using Xunit;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace WebApiTests
{
  public class ExecutorTestFixture : IDisposable
  {
    protected IServiceProvider serviceProvider;
    protected ILogger logger;
    protected IConfigurationStore configStore;

    public static ITagFileAuthProjectV5Proxy tagFileAuthProjectV5Proxy;

    //// this SNM940 exists on `VSS-TagFileAuth-Alpha` with a valid 3d sub (it's not on Dev)
    //// Dims project and customer are on alpha tfa
    //if (request.RadioSerial == "5051593854")
    //  return new GetProjectAndAssetUidsEarthWorksResult(ConstantsUtil.DIMENSIONS_PROJECT_UID, "039c1ee8-1f21-e311-9ee2-00505688274d", ConstantsUtil.DIMENSIONS_CUSTOMER_UID, true);

    public static string dimensionsSerial = "5051593854";
    public static string dimensionsSerialDeviceUid = "039c1ee8-1f21-e311-9ee2-00505688274d";
    public static string dimensionsProjectUid = "ff91dd40-1569-4765-a2bc-014321f76ace";
    public static int dimensionsShortRaptorProjectId = 1001158;
    public static string dimensionsCustomerUID = "87bdf851-44c5-e311-aa77-00505688274d";

    public ExecutorTestFixture()
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
                        .AddTransient<ITagFileAuthProjectV5Proxy, TagFileAuthProjectV5Proxy>()
                        .BuildServiceProvider();

      logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ExecutorTestFixture>();
      configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      tagFileAuthProjectV5Proxy = serviceProvider.GetRequiredService<ITagFileAuthProjectV5Proxy>();
    }

    public void Dispose()
    {
     // do nothing
    }
  }

  [CollectionDefinition("Service collection")]
  public class DatabaseCollection : ICollectionFixture<ExecutorTestFixture>
  {
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
  }
}
