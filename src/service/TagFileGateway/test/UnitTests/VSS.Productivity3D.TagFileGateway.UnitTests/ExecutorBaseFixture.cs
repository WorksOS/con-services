using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.TagFileGateway.Common.Executors;
using VSS.Serilog.Extensions;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.TagFileGateway.UnitTests
{
  public class ExecutorBaseFixture : IDisposable
  {
    public IServiceProvider ServiceProvider { get; }

    public ILoggerFactory LoggerFactory { get; }
    public Mock<IConfigurationStore> ConfigStore { get; }
    public Mock<IDataCache> DataCache { get; }
    public Mock<ITRexTagFileProxy> TRexTagFileProxy { get; }
    public Mock<ITransferProxy> TransferProxy { get; }
    public Mock<ITransferProxyFactory> TransferProxyFactory { get; }
    public Mock<IWebRequest> WebRequest { get; }

    public T CreateExecutor<T>() where T : RequestExecutorContainer, new()
    {
      ConfigStore.Reset();
      DataCache.Reset();

      TRexTagFileProxy.Reset();
      TransferProxy.Reset();
      WebRequest.Reset();
      return RequestExecutorContainer.Build<T>(LoggerFactory, ConfigStore.Object, DataCache.Object, TRexTagFileProxy.Object, TransferProxyFactory.Object, WebRequest.Object);
    }

    public ExecutorBaseFixture()
    {
      LoggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("TagFileGatewayTests.log"));
      ConfigStore = new Mock<IConfigurationStore>(MockBehavior.Strict);
      DataCache = new Mock<IDataCache>(MockBehavior.Strict);
      TRexTagFileProxy = new Mock<ITRexTagFileProxy>(MockBehavior.Strict);
      TransferProxy = new Mock<ITransferProxy>(MockBehavior.Strict);
      TransferProxyFactory = new Mock<ITransferProxyFactory>(MockBehavior.Strict);
      TransferProxyFactory.Setup(x => x.NewProxy(It.IsAny<TransferProxyType>())).Returns(TransferProxy.Object);

      WebRequest = new Mock<IWebRequest>(MockBehavior.Strict);

      ServiceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(LoggerFactory)
        .AddSingleton(ConfigStore.Object)
        .AddSingleton(DataCache.Object)
        .AddSingleton(TRexTagFileProxy.Object)
        .AddSingleton(TransferProxyFactory.Object)
        .AddSingleton(WebRequest.Object)
        .BuildServiceProvider();
    }

    public void Dispose()
    {
    }
  }
}
