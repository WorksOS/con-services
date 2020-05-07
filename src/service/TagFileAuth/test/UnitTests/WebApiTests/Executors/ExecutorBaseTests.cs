using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.RadioSerialMap;
using VSS.Serilog.Extensions;

namespace WebApiTests.Executors
{
  [TestClass]
  public class ExecutorBaseTests
  {
    protected IConfigurationStore ConfigStore;

    protected IServiceProvider ServiceProvider;
    protected Mock<IProjectInternalProxy> projectProxy;
    protected Mock<ICwsAccountClient> cwsAccountClient;
    protected Mock<IDeviceInternalProxy> deviceProxy;
    protected IDictionary<string, string> customHeaders;
    protected static ContractExecutionStatesEnum ContractExecutionStatesEnum = new ContractExecutionStatesEnum();
    protected ILoggerFactory loggerFactory;

    [TestInitialize]
    public virtual void InitTest()
    {
      var serviceCollection = new ServiceCollection();
      
      serviceCollection
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.TagFileAuth.WepApiTests.log")))
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddSingleton<ICustomRadioSerialProjectMap, CustomRadioSerialProjectMap>();

      ServiceProvider = serviceCollection.BuildServiceProvider();
      ConfigStore = ServiceProvider.GetRequiredService<IConfigurationStore>();

      projectProxy = new Mock<IProjectInternalProxy>();
      cwsAccountClient = new Mock<ICwsAccountClient>();
      deviceProxy = new Mock<IDeviceInternalProxy>();
      customHeaders = new Dictionary<string, string>();
      loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
    }
  
  }
}
