using System;
using System.Data;
using System.IO;
using System.IO.Enumeration;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.TRex.Gateway.Tests
{
  public class CreateDesignExecutorTests
  {

    private Guid projectUid = Guid.Parse("A11F2458-6666-424F-A995-4426a00771AE");
    private string transferFileName = "TransferTestDesign.ttm";

    [Theory]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", 1, "validFileName.ttm", "408A150C-B606-E311-9E53-0050568824D7")]
    public void CreateDesignRequestValidation_HappyPath(string projectUid, int fileType, string fileName, string designUid)
    {
      DesignRequest designSurfaceRequest = new DesignRequest(Guid.Parse(projectUid), (ImportedFileType) fileType, fileName, Guid.Parse(designUid), null);
      designSurfaceRequest.Validate();
    }

    [Theory]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", 1, "invalidFileName.dxf", "408A150C-B606-E311-9E53-0050568824D7", -1, "File name extension must be ttm")]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", 1, "", "408A150C-B606-E311-9E53-0050568824D7", -1, "File name must be provided")]
    [InlineData("00000000-0000-0000-0000-000000000000", 1, "validFileName.ttm", "408A150C-B606-E311-9E53-0050568824D7", -1, "ProjectUid must be provided")]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", 1, "validFileName.ttm", "00000000-0000-0000-0000-000000000000", -1, "DesignUid must be provided")]
    public void CreateDesignRequestValidation_Errors(string projectUid, int fileType, string fileName, string designUid, int expectedCode, string expectedMessage)
    {
      DesignRequest designSurfaceRequest = new DesignRequest(Guid.Parse(projectUid), (ImportedFileType)fileType, fileName, Guid.Parse(designUid), null);

      var ex = Assert.Throws<ServiceException>(() => designSurfaceRequest.Validate());
      Assert.Equal(expectedCode, ex.GetResult.Code);
      Assert.Equal(expectedMessage, ex.GetResult.Message);
    }


    [Fact]
    public void FileTransfer_HappyPath()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
        .Add(x => x.AddSingleton<ITransferProxy, TransferProxy>())
        .Complete();

      var isWrittenToS3Ok = S3FileTransfer.WriteFile("TestData", projectUid, transferFileName);
      Assert.True(isWrittenToS3Ok);

      var isReadFromS3Ok = S3FileTransfer.ReadFile(projectUid, transferFileName, Path.GetTempPath()).Result;
      Assert.True(isReadFromS3Ok);
    }

    //[Fact]
    //public void AddDesign_HappyPath()
    //{
    //  SetupDI();
    //  TRexServerConfig.PersistentCacheStoreLocation = Path.GetTempPath();
    //  Guid designUid = Guid.NewGuid();
    //  DesignSurfaceRequest request = new DesignSurfaceRequest(projectUid, transferFileName, designUid);

    //  var executor =
    //    RequestExecutorContainer.Build<UpsertDesignExecutor>(DIContext.Obtain<IConfigurationStore>(), DIContext.Obtain<ILoggerFactory>(), DIContext.Obtain<IServiceExceptionHandler>());
    //  var result = executor.Process(request);

    //  Assert.Equal(0, result.Code);
    //  Assert.Equal("success", result.Message);
    //}

    //private void SetupDI(string sourceFullPath = null)
    //{
    //  //var moqtransferProxy = new Mock<ITransferProxy>();
    //  //var mockReadStream = new Mock<Stream>();
    //  //byte[] bytes = new byte[] { 5,6,7,8 };
    //  //mockReadStream.SetupSequence(s => s.ReadAsync(It.IsAny<byte[]>(), 0, 0x1000, CancellationToken.None))
    //  //  .Returns(Task.FromResult(0x1000));
    //  //  .Returns(Task.FromResult(0x500))
    //  //  .Returns(Task.FromResult(0));
    //  //mockReadStream.Setup(mk => mk.SetLength(5));
    //  //var sourceFileStream = new FileStreamResult(mockReadStream.Object, "text/plain");
    //  //  moqtransferProxy.Setup(mk => mk.Download(It.IsAny<string>())).Returns(Task.FromResult(sourceFileStream));
    //  //  moqtransferProxy.Setup(mk => mk.Upload(It.IsAny<Stream>(), It.IsAny<string>()));

    //  DIBuilder
    //    .New()
    //    .AddLogging()
    //    .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
    //    .Add(x => x.AddTransient<IErrorCodesProvider, ContractExecutionStatesEnum>())
    //    .Add(x => x.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>())
    //    .Add(x => x.AddSingleton<ITransferProxy, TransferProxy>())
    //    .Add(x => x.AddSingleton<ITRexGridFactory>(new TRexGridFactory()))
    //    .Build();


    //  var storageProxy = new StorageProxy_Ignite_Transactional(StorageMutability.Mutable);
    //  storageProxy.SetImmutableStorageProxy(new StorageProxy_Ignite_Transactional(StorageMutability.Immutable));

    //  var moqStorageProxyFactory = new Mock<IStorageProxyFactory>();
    //  moqStorageProxyFactory.Setup(mk => mk.Storage(StorageMutability.Immutable)).Returns(storageProxy);
    //  moqStorageProxyFactory.Setup(mk => mk.Storage(StorageMutability.Mutable)).Returns(storageProxy);
    //  moqStorageProxyFactory.Setup(mk => mk.MutableGridStorage()).Returns(storageProxy);
    //  moqStorageProxyFactory.Setup(mk => mk.ImmutableGridStorage()).Returns(storageProxy);

    //  //var moqSurveyedSurfaces = new Mock<ISurveyedSurfaces>();
    //  var moqSiteModels = new Mock<ISiteModels>();
    //  moqSiteModels.Setup(mk => mk.StorageProxy).Returns(storageProxy);

    //  DIBuilder
    //    .Continue()
    //    .Add(x => x.AddSingleton<IStorageProxy>(storageProxy))
    //    .Add(x => x.AddSingleton<IStorageProxyFactory>(moqStorageProxyFactory.Object))
    //    .Add(x => x.AddSingleton<IDesignManager>(factory => new DesignManager()))
    //    .Add(x => x.AddTransient<IDesigns>(factory => new Designs.Storage.Designs()))
    //    .Add(x => x.AddSingleton<ISiteModels>(moqSiteModels.Object))
    //    //  .Add(x => x.AddSingleton<ISurveyedSurfaces>(moqSurveyedSurfaces.Object))
    //    //  .Add(x => x.AddSingleton<IProductionEventsFactory>(new ProductionEventsFactory()))
    //    .Add(x => x.AddSingleton<IMutabilityConverter>(new MutabilityConverter()))
    //    .Complete();
    //}
  }
}
