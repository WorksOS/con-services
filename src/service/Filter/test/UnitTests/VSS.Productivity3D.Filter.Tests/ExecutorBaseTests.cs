using System;
using CCSS.CWS.Client;
using CCSS.CWS.Client.MockClients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.Common.ServiceDiscovery;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Repository;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Proxy;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Proxy;
using VSS.Serilog.Extensions;
using VSS.Visionlink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Productivity3D.Filter.Tests
{
  public class ExecutorBaseTests : IDisposable
  {
    public IServiceProvider serviceProvider;
    public IServiceExceptionHandler serviceExceptionHandler;

    public ExecutorBaseTests()
    {
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging()
                       .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Productivity3D.Filter.UnitTests.log")))
                       .AddSingleton<IConfigurationStore, GenericConfiguration>()
                       .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()

                       // Required for TIDAuthentication  
                       // CCSSSCON-216 temporary move to MockProjectWebApi when real is available
                       .AddTransient<ICwsAccountClient, MockCwsAccountClient>()

                       .AddTransient<IProductivity3dV2ProxyNotification, Productivity3dV2ProxyNotification>()
                       .AddTransient<IProductivity3dV2ProxyCompaction, Productivity3dV2ProxyCompaction>()
                       .AddTransient<IErrorCodesProvider, FilterErrorCodesProvider>()
                       .AddTransient<IRepository<IFilterEvent>, FilterRepository>()
                       .AddServiceDiscovery()
                       .AddTransient<IProjectProxy, ProjectV6Proxy>();

      serviceProvider = serviceCollection.BuildServiceProvider();

      serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
    }

    public void Dispose()
    { }
  }
}
