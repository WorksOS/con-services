using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Serilog.Extensions;
using VSS.Tile.Service.Common.Services;
using Xunit;

namespace VSS.Tile.Service.UnitTests.Service.Common
{
  public class MapOverlayTests 
  {
    public IServiceProvider serviceProvider;

    public MapOverlayTests()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Project.UnitTests.log"));
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, ContractExecutionStatesEnum>()
        .AddTransient<IMemoryCache, MemoryCache>()
        .AddTransient<IMapTileService, MapTileService>();

      serviceProvider = serviceCollection.BuildServiceProvider();

      _ = serviceProvider.GetRequiredService<ILoggerFactory>();
    }

    [Fact]
    public void CanResolveRegion()
    {
      var region = serviceProvider.GetRequiredService<IMapTileService>().GetRegion(-43, 172);
      Assert.Equal("OC",region);
    }
  }
}
