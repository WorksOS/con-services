using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.TRex.DI;
using VSS.TRex.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.Tests.TestFixtures
{
  public class DILoggingAndStorgeProxyFixture : IDisposable
  {
    private static object Lock = new object();

    public DILoggingAndStorgeProxyFixture()
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
          .Add(x => x.AddSingleton<IStorageProxyFactory>(moqStorageProxyFactory.Object))
          .Complete();
      }
    }

    public void Dispose() { } // Nothing needing doing 
  }
}
