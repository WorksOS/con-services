using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.TRex.DI;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.Tests.TestFixtures
{
  public class DILoggingAndStorageProxyFixture : DILoggingFixture, IDisposable
  {
    public DILoggingAndStorageProxyFixture()
    {
      var moqStorageProxy = new Mock<IStorageProxy>();

      var moqStorageProxyFactory = new Mock<IStorageProxyFactory>();
      moqStorageProxyFactory.Setup(mk => mk.Storage(StorageMutability.Immutable)).Returns(moqStorageProxy.Object);
      moqStorageProxyFactory.Setup(mk => mk.Storage(StorageMutability.Mutable)).Returns(moqStorageProxy.Object);

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton<IStorageProxyFactory>(moqStorageProxyFactory.Object))
        .Complete();
    }

    public new void Dispose()
    {
      base.Dispose();
    }
  }
}
