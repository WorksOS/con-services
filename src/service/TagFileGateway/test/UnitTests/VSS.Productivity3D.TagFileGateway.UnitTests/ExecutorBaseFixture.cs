using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.TagFileGateway.Common.Abstractions;
using VSS.Serilog.Extensions;

namespace VSS.Productivity3D.TagFileGateway.UnitTests
{
  public class ExecutorBaseFixture : IDisposable
  {
    public IServiceProvider ServiceProvider { get; }

    public ILoggerFactory LoggerFactory { get; }
    public Mock<IConfigurationStore> ConfigStore { get; }
    public Mock<IDataCache> DataCache { get; }
    public Mock<ITagFileForwarder> TagFileForwarder { get; }
    public Mock<ITransferProxy> TransferProxy { get; }
    public Mock<IWebRequest> WebRequest { get; }

    public ExecutorBaseFixture()
    {
      LoggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("TagFileGatewayTests.log"));
      ConfigStore = new Mock<IConfigurationStore>();
      DataCache = new Mock<IDataCache>();
      TagFileForwarder = new Mock<ITagFileForwarder>();
      TransferProxy = new Mock<ITransferProxy>();
      WebRequest = new Mock<IWebRequest>();

      ServiceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(LoggerFactory)
        .AddSingleton(ConfigStore.Object)
        .AddSingleton(DataCache.Object)
        .AddSingleton(TagFileForwarder.Object)
        .AddSingleton(TransferProxy.Object)
        .AddSingleton(WebRequest.Object)
        .BuildServiceProvider();
    }

    public void Dispose()
    { }
  }
}
