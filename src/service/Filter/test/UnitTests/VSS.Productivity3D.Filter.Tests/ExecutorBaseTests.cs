using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.Common.ServiceDiscovery;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Proxy;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Proxy;
using VSS.Serilog.Extensions;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

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
                       .AddTransient<ICustomerProxy, CustomerProxy>()
                       .AddTransient<IRaptorProxy, RaptorProxy>()
                       .AddTransient<IErrorCodesProvider, FilterErrorCodesProvider>()
                       .AddTransient<IRepository<IFilterEvent>, FilterRepository>()
                       .AddServiceDiscovery()
                       .AddTransient<IProjectProxy, ProjectV4ServiceDiscoveryProxy>();
   
      serviceProvider = serviceCollection.BuildServiceProvider();

      serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
    }

    public void Dispose()
    { }
  }
}
