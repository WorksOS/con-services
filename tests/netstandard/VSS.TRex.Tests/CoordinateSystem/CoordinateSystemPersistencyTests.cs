using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;
using VSS.TRex.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.CoordinateSystem
{
  public class CoordinateSystemsTestsDIFixture : IDisposable
  {
    private static object Lock = new object();

    public static Guid NewSiteModelGuid = Guid.NewGuid();

    public CoordinateSystemsTestsDIFixture()
    {
      lock (Lock)
      {
        MemoryStream csibStream = new MemoryStream(Encoding.ASCII.GetBytes("MyCSIB"));

        var moqStorageProxy = new Mock<IStorageProxy>();
        moqStorageProxy.Setup(mk => mk.ReadStreamFromPersistentStore(NewSiteModelGuid, CoordinateSystemConsts.kCoordinateSystemCSIBStorageKeyName,
          FileSystemStreamType.CoordinateSystemCSIB, out csibStream)).Returns( FileSystemErrorStatus.OK );

        var moqStorageProxyFactory = new Mock<IStorageProxyFactory>();
        moqStorageProxyFactory.Setup(mk => mk.Storage(StorageMutability.Immutable)).Returns(moqStorageProxy.Object);
        moqStorageProxyFactory.Setup(mk => mk.Storage(StorageMutability.Mutable)).Returns(moqStorageProxy.Object);

        ISiteModel mockedSiteModel = new SiteModel(NewSiteModelGuid);

        var moqSiteModels = new Mock<ISiteModels>();
        moqSiteModels.Setup(mk => mk.GetSiteModel(NewSiteModelGuid)).Returns(mockedSiteModel);
        moqSiteModels.Setup(mk => mk.ImmutableStorageProxy).Returns(moqStorageProxy.Object);

        // Mock the new sitemodel creation API to return jsut a new sitemodel
        moqSiteModels.Setup(mk => mk.GetSiteModel(moqStorageProxy.Object, NewSiteModelGuid, true)).Returns(mockedSiteModel);

        DIBuilder
          .New()
          .AddLogging()
          .Add(x => x.AddSingleton<IStorageProxyFactory>(moqStorageProxyFactory.Object))
          .Add(x => x.AddSingleton<ISiteModels>(moqSiteModels.Object))
          .Complete();
      }
    }
    public void Dispose() { } // Nothing needing doing 
  }

  public class CoordinateSystemPersistencyTests : IClassFixture<CoordinateSystemsTestsDIFixture>
    {
      [Fact(Skip = "Not implemented")]
      public void Test_CoordinateSystemPersistency_Write()
      {
         // Request a storage proxy to implement, thus an Ignite node... leave for now,
         // perhaps implement as an acceptance test?
      }

      [Fact]
      public void Test_CoordinateSystemPersistency_Read()
      {
        // MemoryStream csibStream;
        // SiteModels.SiteModels.Instance().ImmutableStorageProxy.ReadStreamFromPersistentStore
        // (CoordinateSystemsTestsDIFixture.NewSiteModelGuid, CoordinateSystemConsts.kCoordinateSystemCSIBStorageKeyName,
        //  FileSystemStreamType.CoordinateSystemCSIB, out csibStream);

        SiteModel siteModel = new SiteModel("TestName", "TestDesc", CoordinateSystemsTestsDIFixture.NewSiteModelGuid, 1.0);

        string CSIB = siteModel.CSIB();

        Assert.NotNull(CSIB);
        Assert.True(CSIB == "MyCSIB", $"CSIB string not read as expected, read: {CSIB} versus 'MyCSIB'");
    }
  }
}
