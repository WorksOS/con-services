using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.ConfigurationStore;
using VSS.TRex.DI;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.Tests.TestFixtures
{
  public class DILoggingAndStorageProxyFixture : IDisposable
  {
    private static object Lock = new object();

    public DILoggingAndStorageProxyFixture()
    {
      lock (Lock)
      {
        var moqStorageProxy = new Mock<IStorageProxy>();

        var moqStorageProxyFactory = new Mock<IStorageProxyFactory>();
        moqStorageProxyFactory.Setup(mk => mk.Storage(StorageMutability.Immutable)).Returns(moqStorageProxy.Object);
        moqStorageProxyFactory.Setup(mk => mk.Storage(StorageMutability.Mutable)).Returns(moqStorageProxy.Object);

        DIBuilder
          .New()
          .AddLogging()
          .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
          .Add(x => x.AddSingleton<IStorageProxyFactory>(moqStorageProxyFactory.Object))
          .Complete();
      }
    }

    public void Dispose()
    {
      DIBuilder.Continue().Eject();
    } 
  }
}
