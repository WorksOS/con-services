using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.RadioSerialMap;
using VSS.Serilog.Extensions;
using VSS.WebApi.Common;

namespace WebApiTests.Executors
{
  [TestClass]
  public class ExecutorBaseTests
  {
    protected IConfigurationStore ConfigStore;

    protected IServiceProvider ServiceProvider;
    protected Mock<IProjectInternalProxy> projectProxy;
    protected Mock<ITPaaSApplicationAuthentication> authorization;
    protected Mock<ICwsAccountClient> cwsAccountClient;
    protected Mock<IDeviceInternalProxy> deviceProxy;
    protected IHeaderDictionary requestCustomHeaders;
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
      requestCustomHeaders = new HeaderDictionary();
      loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
      authorization = new Mock<ITPaaSApplicationAuthentication>();

      authorization.Setup(x => x.CustomHeaders()).Returns(new HeaderDictionary
      {
        { HeaderConstants.CONTENT_TYPE, ContentTypeConstants.ApplicationJson },
        { HeaderConstants.AUTHORIZATION, "Bearer TOKEN" }
      });
    }
  }
}
